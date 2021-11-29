using LateCat.PoseidonEngine.Abstractions;
using LateCat.PoseidonEngine.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LateCat.Core
{
    public class VideoVlcPlayer : IWallpaper
    {
        public event EventHandler<WindowInitializedArgs> WindowInitialized;

        private IntPtr _hWnd;
        private readonly Process _process;
        private readonly IWallpaperMetadata _metadata;
        private IMonitor _monitor;
        private readonly CancellationTokenSource _ctsProcessWait = new CancellationTokenSource();
        private Task _processWaitTask;
        private readonly int _timeOut;

        public bool IsLoaded => _hWnd != IntPtr.Zero;

        public WallpaperType WallpaperType => _metadata.WallpaperInfo.Type;

        public IWallpaperMetadata Metadata => _metadata;

        public IntPtr Handle => _hWnd;

        public IntPtr InputHandle => IntPtr.Zero;

        public Process Proc => _process;

        public IMonitor Monitor { get => _monitor; set => _monitor = value; }

        public string PropertyCopyPath => null;

        public VideoVlcPlayer(string path, IWallpaperMetadata metadata, IMonitor monitor, WallpaperScaler scaler = WallpaperScaler.Fill, bool hwAccel = true)
        {
            var scalerArg = scaler switch
            {
                WallpaperScaler.None => "--no-autoscale ",
                WallpaperScaler.Fill => "--aspect-ratio=" + monitor.Bounds.Width + ":" + monitor.Bounds.Height,
                WallpaperScaler.Uniform => "--autoscale",
                WallpaperScaler.UniformToFill => "--crop=" + monitor.Bounds.Width + ":" + monitor.Bounds.Height,
                _ => "--autoscale",
            };

            StringBuilder cmdArgs = new StringBuilder();
            //--no-video-title.
            cmdArgs.Append("--no-osd ");
            //video stretch algorithm.
            cmdArgs.Append(scalerArg + " ");
            //hide menus and controls.
            cmdArgs.Append("--qt-minimal-view ");
            //do not create system-tray icon.
            cmdArgs.Append("--no-qt-system-tray ");
            //prevent player window resizing to video size.
            cmdArgs.Append("--no-qt-video-autoresize ");
            //allow Monitorsaver.
            cmdArgs.Append("--no-disable-Monitorsaver ");
            //open window at (-9999,0), not working without: --no-embedded-video
            cmdArgs.Append("--video-x=-9999 --video-y=0 ");
            //gpu decode preference.
            cmdArgs.Append(hwAccel ? "--avcodec-hw=any " : "--avcodec-hw=none ");
            //media file path.
            cmdArgs.Append("\"" + path + "\"");

            ProcessStartInfo start = new ProcessStartInfo
            {
                FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", "vlc", "vlc.exe"),
                UseShellExecute = false,
                WorkingDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", "vlc"),
                Arguments = cmdArgs.ToString(),
            };

            Process _process = new Process()
            {
                EnableRaisingEvents = true,
                StartInfo = start,
            };

            this._process = _process;
            _metadata = metadata;
            _monitor = monitor;
            _timeOut = 20000;
        }

        public async void Close()
        {
            TaskProcessWaitCancel();
            while (!IsProcessWaitDone())
            {
                await Task.Delay(1);
            }

            //Not reliable, app may refuse to close(open dialogue window.. etc)
            //Proc.CloseMainWindow();
            Terminate();
        }

        public WallpaperType GetWallpaperType()
        {
            return _metadata.WallpaperInfo.Type;
        }

        public void Pause()
        {
            //todo
        }

        public void Play()
        {
            //todo
        }

        public void SendMessage(string msg)
        {
            //todo
        }

        public void SetPlaybackPos(float pos, PlaybackPosType type)
        {
            //todo
        }

        public void SetVolume(int volume)
        {
            //todo
        }

        public async void Show()
        {
            if (_process != null)
            {
                try
                {
                    _process.Exited += Proc_Exited;
                    _process.Start();
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
                        //Program ready!
                        WindowInitialized?.Invoke(this, new WindowInitializedArgs()
                        {
                            Success = true,
                            Error = null,
                            Message = null
                        });
                        //todo: Restore Properties.json settings here..
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
                    //No GUI, program failed to enter idle state.
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

        private void Proc_Exited(object sender, EventArgs e)
        {
            _process?.Dispose();

            DesktopUtil.RefreshDesktop();
        }

        #region process task

        /// <summary>
        /// Function to search for window of spawned program.
        /// </summary>
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
                if (!Equals((wHWND = GetProcessWindow(_process, true)), IntPtr.Zero))
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
            var HWND = IntPtr.Zero;

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
                if ((task.IsCompleted == false
                || task.Status == TaskStatus.Running
                || task.Status == TaskStatus.WaitingToRun
                || task.Status == TaskStatus.WaitingForActivation))
                {
                    return false;
                }
                return true;
            }
            return true;
        }

        #endregion process task


        public void Stop()
        {
            //todo
        }

        public void Terminate()
        {
            try
            {
                _process.Kill();
            }
            catch { }
            DesktopUtil.RefreshDesktop();
        }

        public Task Capture(string filePath)
        {
            throw new NotImplementedException();
        }

        public void SendMessage(IPCMessage obj)
        {
            //todo
        }
    }
}
