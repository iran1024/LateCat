﻿using LateCat.Core.Cef;
using LateCat.Helpers;
using LateCat.PoseidonEngine;
using LateCat.PoseidonEngine.Abstractions;
using LateCat.PoseidonEngine.Core;
using LateCat.PoseidonEngine.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LateCat.Core
{
    public class VideoMpvPlayer : IWallpaper
    {
        private class MpvCommand
        {
            [JsonProperty("command")]
            public List<object> Command { get; } = new List<object>();
        }

        public event EventHandler<WindowInitializedArgs> WindowInitialized;

        private IntPtr _hWnd;
        private readonly Process _process;
        private readonly IWallpaperMetadata _metadata;
        private IMonitor _monitor;
        private readonly CancellationTokenSource _ctsProcessWait = new();
        private Task _processWaitTask;
        private readonly int _timeOut;
        private readonly string _ipcServerName;
        private bool _isVideoStopped;
        private JObject _propertiesData;
        private static int _globalCount;
        private readonly int _uniqueId;

        public string PropertyCopyPath { get; }

        public bool IsLoaded { get; private set; } = false;

        public Process Proc => _process;

        public WallpaperType WallpaperType => _metadata.WallpaperInfo.Type;

        public IWallpaperMetadata Metadata => _metadata;

        public IntPtr Handle => _hWnd;

        public IntPtr InputHandle => IntPtr.Zero;

        public IMonitor Monitor { get => _monitor; set => _monitor = value; }

        public VideoMpvPlayer(string path, IWallpaperMetadata metadata, IMonitor monitor, string propertyPath,
            WallpaperScaler scaler = WallpaperScaler.Fill, bool hwAccel = true, bool onMonitorControl = false, StreamQualitySuggestion streamQuality = StreamQualitySuggestion.Highest)
        {
            PropertyCopyPath = propertyPath;

            if (PropertyCopyPath != null)
            {
                _propertiesData = PropertiesJsonHelper.LoadProperties(PropertyCopyPath);
            }

            var scalerArg = scaler switch
            {
                WallpaperScaler.None => "--video-unscaled=yes",
                WallpaperScaler.Fill => "--keepaspect=no",
                WallpaperScaler.Uniform => "--keepaspect=yes",
                WallpaperScaler.UniformToFill => "--panscan=1.0",
                _ => "--keepaspect=no",
            };

            _ipcServerName = "mpvsocket" + Path.GetRandomFileName();
            var configDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", "mpv", "portable_config");

            var cmdArgs = new StringBuilder();

            cmdArgs.Append("--volume=0 ");

            cmdArgs.Append("--loop-file ");

            cmdArgs.Append("--keep-open ");

            cmdArgs.Append("--geometry=-9999:0 ");

            cmdArgs.Append("--force-window=yes ");

            cmdArgs.Append("--no-window-dragging ");

            cmdArgs.Append("--cursor-autohide=no ");

            cmdArgs.Append("--stop-screensaver=no ");

            cmdArgs.Append("--input-default-bindings=no ");

            cmdArgs.Append(scalerArg + " ");

            cmdArgs.Append(!onMonitorControl ? "--no-osc " : " ");

            cmdArgs.Append("--input-ipc-server=" + _ipcServerName + " ");

            cmdArgs.Append(metadata.WallpaperInfo.Type == WallpaperType.Gif ? "--scale=nearest " : " ");

            cmdArgs.Append(hwAccel ? "--hwdec=auto-safe " : "--hwdec=no ");

            cmdArgs.Append(Directory.Exists(configDir) ? "--config-dir=" + "\"" + configDir + "\" " : "--no-config ");

            cmdArgs.Append("--screenshot-template=" + "\"" + Path.Combine(Constants.Paths.TempDir, _ipcServerName) + "\" --screenshot-format=jpg ");

            cmdArgs.Append(metadata.WallpaperInfo.Type == WallpaperType.VideoStream ? StreamHelper.YoutubeDLMpvArgGenerate(streamQuality, path) : "\"" + path + "\"");

            var start = new ProcessStartInfo
            {
                FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", "mpv", "mpv.exe"),
                UseShellExecute = false,
                RedirectStandardError = false,
                RedirectStandardInput = false,
                RedirectStandardOutput = true,
                WorkingDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", "mpv"),
                Arguments = cmdArgs.ToString(),
            };

            var _process = new Process()
            {
                EnableRaisingEvents = true,
                StartInfo = start,
            };

            this._process = _process;
            _metadata = metadata;
            _monitor = monitor;
            _timeOut = 20000;

            _uniqueId = _globalCount++;
        }

        public async void Close()
        {
            TaskProcessWaitCancel();

            while (!IsProcessWaitDone())
            {
                await Task.Delay(1);
            }

            Terminate();
        }

        public void Play()
        {
            if (_isVideoStopped)
            {
                _isVideoStopped = false;

                SendMessage("{\"command\":[\"set_property\",\"vid\",1]}\n");
            }
            SendMessage("{\"command\":[\"set_property\",\"pause\",false]}\n");
        }

        public void Pause()
        {
            SendMessage("{\"command\":[\"set_property\",\"pause\",true]}\n");
        }

        public void Stop()
        {
            _isVideoStopped = true;

            SendMessage("{\"command\":[\"set_property\",\"vid\",\"no\"]}\n");
            Pause();
        }

        public void SetVolume(int volume)
        {
            SendMessage("{\"command\":[\"set_property\",\"volume\"," + JsonConvert.SerializeObject(volume) + "]}\n");
        }

        public void SetPlaybackPos(float pos, PlaybackPosType type)
        {
            if (WallpaperType != WallpaperType.Picture)
            {
                var posStr = JsonConvert.SerializeObject(pos);
                switch (type)
                {
                    case PlaybackPosType.Absolute:
                        SendMessage("{\"command\":[\"seek\"," + posStr + ",\"absolute-percent\"]}\n");
                        break;
                    case PlaybackPosType.Relative:
                        SendMessage("{\"command\":[\"seek\"," + posStr + ",\"relative-percent\"]}\n");
                        break;
                }
            }
        }

        public async Task Capture(string filePath)
        {
            if (WallpaperType == WallpaperType.Gif)
            {
                await Task.Run(() =>
                {
                    if (OperatingSystem.IsWindowsVersionAtLeast(7))
                    {
                        using var img = Image.FromFile(Metadata.FilePath);

                        img.SelectActiveFrame(new FrameDimension(img.FrameDimensionsList[0]), 0);

                        img.Save(Path.GetExtension(filePath) != ".jpg" ? filePath + ".jpg" : filePath, ImageFormat.Jpeg);
                    }
                });
            }
            else
            {
                var tcs = new TaskCompletionSource<bool>();
                var imgPath = Path.Combine(Constants.Paths.TempDir, _ipcServerName + ".jpg");

                using var watcher = new FileSystemWatcher();
                watcher.Path = Constants.Paths.TempDir;
                watcher.NotifyFilter = NotifyFilters.LastWrite;
                watcher.Filter = "*.jpg";
                watcher.Changed += (s, e) =>
                {
                    if (Path.GetFileName(e.FullPath) == Path.GetFileName(imgPath) && e.ChangeType == WatcherChangeTypes.Changed)
                    {
                        File.Move(imgPath, Path.GetExtension(filePath) != ".jpg" ? filePath + ".jpg" : filePath, true);
                        tcs.SetResult(true);
                    }
                };
                watcher.EnableRaisingEvents = true;

                using var timer = new System.Windows.Forms.Timer()
                {
                    Enabled = true,
                    Interval = 10000,
                };
                timer.Tick += (s, e) =>
                {
                    tcs.SetResult(false);
                };

                SendMessage("{\"command\":[\"screenshot\",\"video\"]}\n");
                await tcs.Task;
            }
        }

        private void SetPlaybackProperties(JObject property)
        {
            if (property is null)
            {
                return;
            }

            try
            {
                string msg;
                foreach (var item in property)
                {
                    string uiElement = item.Value["type"].ToString();
                    if (!uiElement.Equals("button", StringComparison.OrdinalIgnoreCase) && !uiElement.Equals("label", StringComparison.OrdinalIgnoreCase))
                    {
                        msg = null;
                        if (uiElement.Equals("slider", StringComparison.OrdinalIgnoreCase))
                        {
                            msg = GetMpvCommand("set_property", item.Key, (string)item.Value["value"]);
                        }
                        else if (uiElement.Equals("checkbox", StringComparison.OrdinalIgnoreCase))
                        {
                            msg = GetMpvCommand("set_property", item.Key, (bool)item.Value["value"]);
                        }

                        if (msg != null)
                        {
                            PipeClient.SendMessage(_ipcServerName, msg);
                        }
                    }
                }
            }
            catch
            {
                //todo
            }
        }

        public async void Show()
        {
            if (_process != null)
            {
                try
                {
                    _process.Exited += Proc_Exited;
                    _process.OutputDataReceived += Proc_OutputDataReceived;
                    _process.Start();
                    _process.BeginOutputReadLine();
                    _processWaitTask = Task.Run(() => _hWnd = WaitForProcesWindow().Result, _ctsProcessWait.Token);
                    await _processWaitTask;
                    if (_hWnd.Equals(IntPtr.Zero))
                    {
                        WindowInitialized?.Invoke(this, new WindowInitializedArgs()
                        {
                            Success = false,
                            Error = new Exception(Properties.Resources.ExceptionGeneral),
                            Message = "Process window handle is zero."
                        });
                    }
                    else
                    {
                        WindowOperations.BorderlessWinStyle(_hWnd);
                        WindowOperations.RemoveWindowFromTaskbar(_hWnd);

                        WindowInitialized?.Invoke(this, new WindowInitializedArgs()
                        {
                            Success = true,
                            Error = null,
                            Message = null
                        });

                        SetPlaybackProperties(_propertiesData);

                        await Task.Delay(69);
                        IsLoaded = true;
                    }
                }
                catch (OperationCanceledException e1)
                {
                    WindowInitialized?.Invoke(this, new WindowInitializedArgs()
                    {
                        Success = false,
                        Error = e1,
                        Message = "Program wallpaper terminated early/user cancel."
                    });
                }
                catch (InvalidOperationException e2)
                {
                    WindowInitialized?.Invoke(this, new WindowInitializedArgs()
                    {
                        Success = false,
                        Error = e2,
                        Message = "Program wallpaper crashed/closed already!"
                    });
                }
                catch (Exception e3)
                {
                    WindowInitialized?.Invoke(this, new WindowInitializedArgs()
                    {
                        Success = false,
                        Error = e3,
                        Message = ":("
                    });
                }
            }
        }

        private void Proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {

            }
        }

        private void Proc_Exited(object sender, EventArgs e)
        {
            _process.OutputDataReceived -= Proc_OutputDataReceived;
            _process?.Dispose();
            DesktopUtilities.RefreshDesktop();
        }

        #region process task

        private async Task<IntPtr> WaitForProcesWindow()
        {
            if (_process == null)
            {
                return IntPtr.Zero;
            }

            _process.Refresh();

            while (_process.WaitForInputIdle(-1) != true)
            {
                _ctsProcessWait.Token.ThrowIfCancellationRequested();
            }

            var wHWND = IntPtr.Zero;

            for (int i = 0; i < _timeOut && _process.HasExited == false; i++)
            {
                _ctsProcessWait.Token.ThrowIfCancellationRequested();

                if (!Equals(wHWND = GetProcessWindow(_process, true), IntPtr.Zero))
                    break;

                await Task.Delay(1);
            }

            return wHWND;
        }

        private IntPtr GetProcessWindow(Process proc, bool win32Search = false)
        {
            if (_process == null)
                return IntPtr.Zero;

            if (win32Search)
            {
                return FindWindowByProcessId(proc.Id);
            }
            else
            {
                proc.Refresh();

                return proc.MainWindowHandle;
            }
        }

        private IntPtr FindWindowByProcessId(int pid)
        {
            IntPtr HWND = IntPtr.Zero;
            Win32.EnumWindows(new Win32.EnumWindowsProc((tophandle, topparamhandle) =>
            {
                _ = Win32.GetWindowThreadProcessId(tophandle, out int cur_pid);
                if (cur_pid == pid)
                {
                    if (Win32.IsWindowVisible(tophandle))
                    {
                        HWND = tophandle;
                        return false;
                    }
                }

                return true;
            }), IntPtr.Zero);

            return HWND;
        }

        private void TaskProcessWaitCancel()
        {
            if (_ctsProcessWait == null)
                return;

            _ctsProcessWait.Cancel();
            _ctsProcessWait.Dispose();
        }

        private bool IsProcessWaitDone()
        {
            var task = _processWaitTask;
            if (task != null)
            {
                if (task.IsCompleted == false
                || task.Status == TaskStatus.Running
                || task.Status == TaskStatus.WaitingToRun
                || task.Status == TaskStatus.WaitingForActivation)
                {
                    return false;
                }
                return true;
            }
            return true;
        }

        #endregion process task

        public void Terminate()
        {
            try
            {
                _process.Kill();
            }
            catch { }

            DesktopUtilities.RefreshDesktop();
        }

        public void SendMessage(string msg)
        {
            try
            {
                PipeClient.SendMessage(_ipcServerName, msg);
            }
            catch { }
        }

        public void SendMessage(IPCMessage obj)
        {
            try
            {
                string msg = null;
                switch (obj.Type)
                {
                    case MessageType.lp_slider:
                        var sl = (IPCSlider)obj;
                        if ((sl.Step % 1) != 0)
                        {
                            msg = GetMpvCommand("set_property", sl.Name, sl.Value);
                        }
                        else
                        {
                            msg = GetMpvCommand("set_property", sl.Name, Convert.ToInt32(sl.Value));
                        }
                        break;
                    case MessageType.lp_chekbox:
                        var chk = (IPCCheckbox)obj;
                        msg = GetMpvCommand("set_property", chk.Name, chk.Value);
                        break;
                    case MessageType.lp_button:
                        var btn = (IPCButton)obj;
                        if (btn.IsDefault)
                        {
                            _propertiesData = PropertiesJsonHelper.LoadProperties(PropertyCopyPath);

                            SetPlaybackProperties(_propertiesData);
                        }
                        else { }
                        break;
                    case MessageType.lp_dropdown:
                        break;
                    case MessageType.lp_textbox:
                        break;
                    case MessageType.lp_cpicker:
                        break;
                    case MessageType.lp_fdropdown:
                        break;
                }

                if (msg != null)
                {
                    SendMessage(msg);
                }
            }
            catch (OverflowException)
            {

            }
            catch { }
        }

        #region mpv util

        /*                                      - BenchmarkDotNet -
         *|        Method     |     Mean |     Error |    StdDev |  Gen 0   | Gen 1 | Gen 2 | Allocated |
         *|------------------:|---------:|----------:|----------:|---------:|------:|------:|----------:|
         *| GetMpvCommand     | 1.493 us | 0.0085 us | 0.0080 us | 0.5741   |     - |     - |      2 KB |
         *| GetMpvCommandStrb | 1.551 us | 0.0148 us | 0.0138 us | 1.7033   |     - |     - |      5 KB |
         */

        private string GetMpvCommand(params object[] parameters)
        {
            var obj = new MpvCommand();
            obj.Command.AddRange(parameters);
            return JsonConvert.SerializeObject(obj) + Environment.NewLine;
        }

        private string GetMpvCommandStrb(params object[] parameters)
        {
            var script = new StringBuilder();
            script.Append("{\"command\":[");
            for (int i = 0; i < parameters.Length; i++)
            {
                script.Append(JsonConvert.SerializeObject(parameters[i]));
                if (i < parameters.Length - 1)
                {
                    script.Append(", ");
                }
            }
            script.Append("]}\n");
            return script.ToString();
        }

        #endregion //mpv util
    }
}
