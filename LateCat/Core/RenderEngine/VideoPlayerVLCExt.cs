﻿using LateCat.PoseidonEngine.Abstractions;
using LateCat.PoseidonEngine.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace LateCat.Core
{
    /// <summary>
    /// libVLC videoplayer (External plugin.)
    /// </summary>
    public class VideoPlayerVLCExt : IWallpaper
    {
        private IntPtr _hWnd;
        private readonly Process _process;
        private readonly IWallpaperMetadata _metadata;
        private IMonitor _monitor;
        private bool _initialized;

        public bool IsLoaded => _hWnd != IntPtr.Zero;

        public WallpaperType WallpaperType => _metadata.WallpaperInfo.Type;

        public IWallpaperMetadata Metadata => _metadata;

        public IntPtr Handle => _hWnd;

        public IntPtr InputHandle => IntPtr.Zero;

        public Process Proc => _process;

        public IMonitor Monitor { get => _monitor; set => _monitor = value; }

        public string PropertyCopyPath => null;

        public event EventHandler<WindowInitializedArgs> WindowInitialized;

        public VideoPlayerVLCExt(string path, IWallpaperMetadata model, IMonitor display)
        {
            var start = new ProcessStartInfo
            {
                Arguments = "\"" + path + "\"",
                FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", "libVLCPlayer", "LateCat.LibVLCPlayer.exe"),
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WorkingDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", "LateCat.LibVLCPlayer")
            };

            var _process = new Process
            {
                StartInfo = start,
                EnableRaisingEvents = true
            };

            this._process = _process;
            _metadata = model;
            _monitor = display;
        }

        public void Close()
        {
            try
            {
                _process.Refresh();
                _process.StandardInput.WriteLine("LateCat:terminate");
            }
            catch
            {
                Terminate();
            }
        }

        public void Pause()
        {
            SendMessage("LateCat:vid-pause");
        }

        public void Play()
        {
            SendMessage("LateCat:vid-play");
        }

        public void Show()
        {
            if (_process != null)
            {
                try
                {
                    _process.Exited += Proc_Exited;
                    _process.OutputDataReceived += Proc_OutputDataReceived;
                    _process.Start();
                    _process.BeginOutputReadLine();
                }
                catch (Exception e)
                {
                    WindowInitialized?.Invoke(this, new WindowInitializedArgs() { Success = false, Error = e, Message = "Failed to start process." });
                    Close();
                }
            }
        }

        private void Proc_Exited(object sender, EventArgs e)
        {
            if (!_initialized)
            {
                //Exited with no error and without even firing OutputDataReceived; probably some external factor.
                WindowInitialized?.Invoke(this, new WindowInitializedArgs()
                {
                    Success = false,
                    Error = new Exception(Properties.Resources.ExceptionGeneral),
                    Message = "Process exited before giving HWND."
                });
            }
            _process.OutputDataReceived -= Proc_OutputDataReceived;
            _process?.Dispose();
            DesktopUtilities.RefreshDesktop();
        }

        private void Proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            //When the redirected stream is closed, a null line is sent to the event handler.
            if (!string.IsNullOrEmpty(e.Data))
            {
                if (e.Data.Contains("HWND"))
                {
                    bool status = true;
                    Exception error = null;
                    string msg = null;
                    try
                    {
                        msg = e.Data;
                        var handle = new IntPtr();
                        handle = new IntPtr(Convert.ToInt32(e.Data.Substring(4), 10));
                        if (IntPtr.Equals(handle, IntPtr.Zero))//unlikely.
                        {
                            status = false;
                        }
                        _hWnd = handle;
                    }
                    catch (Exception ex)
                    {
                        status = false;
                        error = ex;
                    }
                    finally
                    {
                        if (!_initialized)
                        {
                            WindowInitialized?.Invoke(this, new WindowInitializedArgs() { Success = status, Error = error, Message = msg });
                        }
                        //First run sent msg will be window handle.
                        _initialized = true;
                    }
                }
            }
        }

        public void Stop()
        {
            //throw new NotImplementedException();
        }

        public void SendMessage(string msg)
        {
            if (_process != null)
            {
                try
                {
                    _process.StandardInput.WriteLine(msg);
                }
                catch { }
            }
        }

        public void SetMonitor(PoseidonMonitor display)
        {
            _monitor = display;
        }

        public void Terminate()
        {
            try
            {
                _process.Kill();
            }
            catch { }
            DesktopUtilities.RefreshDesktop();
        }

        public void SetVolume(int volume)
        {
            SendMessage("LateCat:vid-volume " + volume);
        }

        public void SetPlaybackPos(float pos, PlaybackPosType type)
        {
            //todo
        }

        public async Task Capture(string filePath)
        {
            throw new NotImplementedException();
        }

        public void SendMessage(IPCMessage obj)
        {
            //todo
        }
    }
}
