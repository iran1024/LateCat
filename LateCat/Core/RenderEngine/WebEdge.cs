using LateCat.PoseidonEngine.Abstractions;
using LateCat.PoseidonEngine.Core;
using LateCat.Views;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Threading;

namespace LateCat.Core
{
    public class WebEdge : IWallpaper
    {
        public event EventHandler<WindowInitializedArgs> WindowInitialized;

        private IntPtr _hWndWebView, _hWndWindow;
        private readonly WebView2Element _render;
        private readonly IWallpaperMetadata _metadata;
        private IMonitor _monitor;

        public bool IsLoaded { get; private set; } = false;

        public WallpaperType WallpaperType => _metadata.WallpaperInfo.Type;

        public IWallpaperMetadata Metadata => _metadata;

        public IntPtr Handle => _hWndWindow;

        public IntPtr InputHandle => _hWndWebView;

        public Process Proc => null;

        public IMonitor Monitor { get => _monitor; set => _monitor = value; }

        public string PropertyCopyPath { get; }

        public WebEdge(string path, IWallpaperMetadata model, IMonitor display, string propertyPath)
        {
            PropertyCopyPath = propertyPath;

            _render = new WebView2Element(path, model.WallpaperInfo.Type, PropertyCopyPath);

            _metadata = model;
            _monitor = display;
        }

        public void Close()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new ThreadStart(delegate
            {
                _render.Close();
            }));
        }

        public void Pause()
        {
            Win32.ShowWindow(_hWndWebView, (uint)Win32.SHOWWINDOW.SW_SHOWMINNOACTIVE);
        }

        public void Play()
        {
            Win32.ShowWindow(_hWndWebView, (uint)Win32.SHOWWINDOW.SW_SHOWNOACTIVATE);
        }

        public void SendMessage(string msg)
        {
            _render?.MessageProcess(msg);
        }

        public void SetVolume(int volume)
        {

        }

        public async void Show()
        {
            if (_render != null)
            {
                _render.PropertiesInitialized += Player_PropertiesInitialized;
                _render.Closed += Player_Closed;
                _render.Show();

                _hWndWindow = new WindowInteropHelper(_render).Handle;

                bool status = true;
                Exception error = null;
                string message = null;
                try
                {
                    var tmpHwnd = await _render.InitializeWebView();

                    var parentHwnd = Win32.FindWindowEx(tmpHwnd, IntPtr.Zero, "Chrome_WidgetWin_0", null);
                    if (!parentHwnd.Equals(IntPtr.Zero))
                    {
                        _hWndWebView = Win32.FindWindowEx(parentHwnd, IntPtr.Zero, "Chrome_WidgetWin_1", null);
                    }

                    if (_hWndWebView.Equals(IntPtr.Zero))
                    {
                        throw new Exception("Webview input handle not found.");
                    }
                }
                catch (Exception e)
                {
                    error = e;
                    status = false;
                    message = "WebView initialization fail.";
                }
                finally
                {
                    WindowInitialized?.Invoke(this, new WindowInitializedArgs() { Success = status, Error = error, Message = message });
                }
            }
        }

        private void Player_PropertiesInitialized(object sender, EventArgs e)
        {
            IsLoaded = true;
            _render.PropertiesInitialized -= Player_PropertiesInitialized;
        }

        private void Player_Closed(object sender, EventArgs e)
        {
            DesktopUtilities.RefreshDesktop();
        }

        public void Stop()
        {

        }

        public void Terminate()
        {
            Close();
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
            await _render?.CaptureMonitorshot(Path.GetExtension(filePath) != ".jpg" ? filePath + ".jpg" : filePath, ScreenshotFormat.jpeg);
        }

        public void SendMessage(IPCMessage obj)
        {
            _render?.MessageProcess(obj);
        }
    }
}
