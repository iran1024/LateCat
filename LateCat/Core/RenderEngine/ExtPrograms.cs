using LateCat.PoseidonEngine.Abstractions;
using LateCat.PoseidonEngine.Core;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace LateCat.Core
{
    public class ExtPrograms : IWallpaper
    {
        private IntPtr _hWnd;
        private readonly Process _process;
        private readonly IWallpaperMetadata _metadata;
        private IMonitor _monitor;
        public uint SuspendCnt { get; set; }

        public bool IsLoaded => _hWnd != IntPtr.Zero;

        public WallpaperType WallpaperType => _metadata.WallpaperInfo.Type;

        public IWallpaperMetadata Metadata => _metadata;

        public IntPtr Handle => _hWnd;

        public IntPtr InputHandle => _hWnd;

        public Process Proc => _process;

        public IMonitor Monitor { get => _monitor; set => _monitor = value; }

        public string PropertyCopyPath => null;

        public event EventHandler<WindowInitializedArgs> WindowInitialized;
        private readonly CancellationTokenSource ctsProcessWait = new CancellationTokenSource();
        private Task processWaitTask;
        private readonly int timeOut;


        public ExtPrograms(string path, IWallpaperMetadata model, IMonitor display, int timeOut = 20000)
        {
            var cmdArgs = model.WallpaperInfo.Arguments;

            var start = new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = false,
                WorkingDirectory = System.IO.Path.GetDirectoryName(path),
                Arguments = cmdArgs,
            };

            var _process = new Process()
            {
                EnableRaisingEvents = true,
                StartInfo = start,
            };

            this._process = _process;
            _metadata = model;
            _monitor = display;
            this.timeOut = timeOut;
            SuspendCnt = 0;
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

        public void Pause()
        {
            if (_process != null)
            {

            }
        }

        public void Play()
        {
            if (_process != null)
            {

            }
        }

        public void Stop()
        {

        }

        public async void Show()
        {
            if (_process != null)
            {
                try
                {
                    _process.Exited += Proc_Exited;
                    _process.Start();
                    processWaitTask = Task.Run(() => _hWnd = WaitForProcesWindow().Result, ctsProcessWait.Token);
                    await processWaitTask;
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

        private void Proc_Exited(object sender, EventArgs e)
        {
            _process?.Dispose();
            DesktopUtil.RefreshDesktop(Program.OriginalDesktopWallpaperPath);
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
            //waiting for program messageloop to be ready (GUI is not guaranteed to be ready.)
            while (_process.WaitForInputIdle(-1) != true)
            {
                ctsProcessWait.Token.ThrowIfCancellationRequested();
            }

            IntPtr wHWND = IntPtr.Zero;
            if (WallpaperType == WallpaperType.Godot)
            {
                for (int i = 0; i < timeOut && _process.HasExited == false; i++)
                {
                    ctsProcessWait.Token.ThrowIfCancellationRequested();

                    wHWND = Win32.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Engine", string.Empty);
                    if (!IntPtr.Equals(wHWND, IntPtr.Zero))
                        break;
                    await Task.Delay(1);
                }
                return wHWND;
            }

            for (int i = 0; i < timeOut && _process.HasExited == false; i++)
            {
                ctsProcessWait.Token.ThrowIfCancellationRequested();
                if (!Equals(wHWND = GetProcessWindow(_process, true), IntPtr.Zero))
                    break;

                await Task.Delay(1);
            }


            IntPtr cHWND = Win32.FindWindowEx(wHWND, IntPtr.Zero, "Button", "Play!");
            if (!Equals(cHWND, IntPtr.Zero))
            {
                _ = Win32.SendMessage(cHWND, Win32.BM_CLICK, IntPtr.Zero, IntPtr.Zero);

                wHWND = IntPtr.Zero;

                await Task.Delay(1);

                for (int i = 0; i < timeOut && _process.HasExited == false; i++)
                {
                    ctsProcessWait.Token.ThrowIfCancellationRequested();
                    if (!Equals(wHWND = GetProcessWindow(_process, true), IntPtr.Zero))
                    {
                        break;
                    }
                    await Task.Delay(1);
                }
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
            if (ctsProcessWait == null)
                return;

            ctsProcessWait.Cancel();
            ctsProcessWait.Dispose();
        }

        private bool IsProcessWaitDone()
        {
            var task = processWaitTask;
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

        public void SendMessage(string msg)
        {

        }

        public void Terminate()
        {
            try
            {
                _process.Kill();
            }
            catch { }
            DesktopUtil.RefreshDesktop(Program.OriginalDesktopWallpaperPath);
        }

        public void SetVolume(int volume)
        {
            try
            {
                VolumeMixer.SetApplicationVolume(_process.Id, volume);
            }
            catch { }
        }

        public void SetPlaybackPos(float pos, PlaybackPosType type)
        {
            //todo
        }

        public Task Capture(string filePath)
        {
            throw new NotImplementedException();
        }

        public void SendMessage(IPCMessage obj)
        {

        }
    }
}
