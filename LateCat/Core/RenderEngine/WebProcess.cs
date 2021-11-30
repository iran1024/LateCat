using LateCat.PoseidonEngine;
using LateCat.PoseidonEngine.Abstractions;
using LateCat.PoseidonEngine.Core;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace LateCat.Core
{
    internal class WebProcess : IWallpaper
    {
        private IntPtr _hWndWebView, _hWndWindow;
        private readonly Process _process;
        private readonly IWallpaperMetadata _metadata;
        private IMonitor _monitor;
        private bool _initialized;
        private static int _globalCount;
        private readonly int _uniqueId;

        public event EventHandler<WindowInitializedArgs> WindowInitialized;

        public bool IsLoaded { get; private set; } = false;

        public WallpaperType WallpaperType => _metadata.WallpaperInfo.Type;

        public IWallpaperMetadata Metadata => _metadata;

        public IntPtr Handle => _hWndWindow;

        public IntPtr InputHandle => _hWndWebView;

        public Process Proc => _process;

        public IMonitor Monitor { get => _monitor; set => _monitor = value; }

        public string PropertyCopyPath { get; }

        public WebProcess(string path, IWallpaperMetadata metadata, IMonitor monitor, string propertyPath, bool diskCache, int volume)
        {
            PropertyCopyPath = propertyPath;

            var cmdArgs = new StringBuilder();
            cmdArgs.Append(" --url " + "\"" + path + "\"");
            cmdArgs.Append(" --display " + "\"" + monitor + "\"");
            cmdArgs.Append(" --property " + "\"" + PropertyCopyPath + "\"");
            cmdArgs.Append(" --volume " + volume);
            cmdArgs.Append(" --geometry " + monitor.Bounds.Width + "x" + monitor.Bounds.Height);
            cmdArgs.Append(metadata.WallpaperInfo.Type == WallpaperType.WebAudio ? " --audio true" : " ");
            cmdArgs.Append(!string.IsNullOrWhiteSpace(metadata.WallpaperInfo.Arguments) ? " " + metadata.WallpaperInfo.Arguments : " ");
            cmdArgs.Append(metadata.WallpaperInfo.Type == WallpaperType.Url || metadata.WallpaperInfo.Type == WallpaperType.VideoStream ? " --type online" : " --type local");
            cmdArgs.Append(diskCache && metadata.WallpaperInfo.Type == WallpaperType.Url ? " --cache " + "\"" + Path.Combine(Constants.Paths.TempCefDir, "cache", monitor.DeviceNumber) + "\"" : " ");

            var start = new ProcessStartInfo
            {
                Arguments = cmdArgs.ToString(),
                FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", "cef", "LateCat.CefPlayer.exe"),
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WorkingDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", "cef")
            };

            var webProcess = new Process
            {
                StartInfo = start,
                EnableRaisingEvents = true
            };

            _process = webProcess;
            _metadata = metadata;
            _monitor = monitor;

            _uniqueId = _globalCount++;
        }

        public void Close()
        {
            Terminate();
        }

        public void Pause()
        {
            Win32.ShowWindow(_hWndWebView, (uint)Win32.SHOWWINDOW.SW_SHOWMINNOACTIVE);
        }

        public void Play()
        {
            Win32.ShowWindow(_hWndWebView, (uint)Win32.SHOWWINDOW.SW_SHOWNOACTIVATE);
        }

        private void WallpaperRectFix()
        {
            if (VerifyWindowRect(Handle, Monitor))
            {
                if (!Win32.SetWindowPos(Handle, 1, 0, 0, Monitor.Bounds.Width, Monitor.Bounds.Height, 0 | 0x0010 | 0x0002))
                {

                }
            }
        }

        private static bool VerifyWindowRect(IntPtr hWnd, IMonitor monitor)
        {
            try
            {
                System.Drawing.Rectangle MonitorBounds;
                _ = Win32.GetWindowRect(hWnd, out Win32.RECT appBounds);

                MonitorBounds = System.Windows.Forms.Screen.FromHandle(hWnd).Bounds;
                return ((appBounds.Bottom - appBounds.Top) != monitor.Bounds.Height || (appBounds.Right - appBounds.Left) != monitor.Bounds.Width);
            }
            catch
            {
                return false;
            }
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

        private void Proc_Exited(object? sender, EventArgs e)
        {
            if (!_initialized)
            {
                WindowInitialized?.Invoke(this, new WindowInitializedArgs()
                {
                    Success = false,
                    Error = new Exception("Process"),
                    Message = "Process exited before giving HWND."
                });
            }
            _process.OutputDataReceived -= Proc_OutputDataReceived;
            _process?.Dispose();

            DesktopUtil.RefreshDesktop(Program.OriginalDesktopWallpaperPath);
        }

        private void Proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                if (!_initialized || !IsLoaded)
                {
                    IPCMessage obj;
                    try
                    {
                        obj = JsonConvert.DeserializeObject<IPCMessage>(e.Data, new JsonSerializerSettings() { Converters = { new IPCMessageConverter() } })!;
                    }
                    catch
                    {
                        return;
                    }

                    if (obj.Type == MessageType.msg_hwnd)
                    {
                        var status = true;
                        Exception error = null;
                        var msg = string.Empty;
                        try
                        {
                            msg = e.Data;
                            var handle = new IntPtr(((IPCMessageHwnd)obj).Hwnd);

                            _hWndWebView = Win32.FindWindowEx(handle, IntPtr.Zero, "Chrome_WidgetWin_0", null);

                            _hWndWindow = FindWindowByProcessId(Proc.Id);

                            if (IntPtr.Equals(_hWndWebView, IntPtr.Zero) || IntPtr.Equals(_hWndWindow, IntPtr.Zero))
                            {
                                throw new Exception("Browser input/window handle NULL.");
                            }

                            WindowOperator.RemoveWindowFromTaskbar(_hWndWindow);
                        }
                        catch (Exception ie)
                        {
                            status = false;
                            error = ie;
                        }
                        finally
                        {
                            _initialized = true;
                            WindowInitialized?.Invoke(this, new WindowInitializedArgs() { Success = status, Error = error!, Message = msg });
                        }
                    }
                    else if (obj.Type == MessageType.msg_wploaded)
                    {
                        IsLoaded = ((IPCMessageWallpaperLoaded)obj).Success;
                    }
                }
            }
        }

        private static IntPtr FindWindowByProcessId(int pid)
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

        public void Stop()
        {

        }

        public void SendMessage(string msg)
        {
            try
            {
                _process?.StandardInput.WriteLine(msg);
            }
            catch
            {

            }
        }

        public void SendMessage(IPCMessage obj)
        {
            SendMessage(JsonConvert.SerializeObject(obj));
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

        public void Resume()
        {
            //throw new NotImplementedException();
        }

        public void SetVolume(int volume)
        {
            /*
            try
            {
                if (Proc != null)
                {
                    //VolumeMixer.SetApplicationVolume(Proc.Id, volume);
                    SetProcessAndChildrenVolume(Proc.Id, volume);
                }
            }
            catch { }
            */
        }

        private void SetProcessAndChildrenVolume(int pid, int volume)
        {
            var searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + pid);
            var moc = searcher.Get();

            foreach (ManagementObject mo in moc)
            {
                SetProcessAndChildrenVolume(Convert.ToInt32(mo["ProcessID"]), volume);
            }

            VolumeMixer.SetApplicationVolume(Process.GetProcessById(pid).Id, volume);
        }

        private void KillProcessAndChildren(int pid)
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc)
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }

            try
            {
                Process proc = Process.GetProcessById(pid);
                proc.Kill();
            }
            catch (ArgumentException)
            {

            }
        }

        public void SetPlaybackPos(float pos, PlaybackPosType type)
        {
            if (pos == 0 && type != PlaybackPosType.Relative)
            {
                SendMessage(new IPCReloadCmd());
            }
        }

        public async Task Capture(string filePath)
        {
            var tcs = new TaskCompletionSource<bool>();

            void OutputDataReceived(object sender, DataReceivedEventArgs e)
            {
                if (string.IsNullOrEmpty(e.Data))
                {
                    tcs.SetResult(false);
                }
                else
                {
                    var obj = JsonConvert.DeserializeObject<IPCMessage>(e.Data, new JsonSerializerSettings() { Converters = { new IPCMessageConverter() } });
                    if (obj?.Type == MessageType.msg_screenshot)
                    {
                        var msg = (IPCMessageScreenshot)obj;
                        if (msg.FileName == Path.GetFileName(filePath))
                        {
                            tcs.SetResult(msg.Success);
                        }
                    }
                }
            }

            try
            {
                _process.OutputDataReceived += OutputDataReceived;
                SendMessage(new IPCMonitorshotCmd()
                {
                    FilePath = Path.GetExtension(filePath) != ".jpg" ? filePath + ".jpg" : filePath,
                    Format = ScreenshotFormat.jpeg,
                    Delay = 0
                });

                await tcs.Task;
            }
            finally
            {
                _process.OutputDataReceived -= OutputDataReceived;
            }
        }
    }
}
