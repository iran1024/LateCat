using LateCat.PoseidonEngine.Abstractions;
using LateCat.PoseidonEngine.Core;
using Linearstar.Windows.RawInput;
using System;
using System.Windows;
using System.Windows.Interop;

namespace LateCat.Core
{
    public enum RawInputMouseBtn
    {
        left,
        right
    }

    public class MouseRawArgs : EventArgs
    {
        public int X { get; }
        public int Y { get; }
        public MouseRawArgs(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public class MouseClickRawArgs : MouseRawArgs
    {
        public RawInputMouseBtn Button { get; }
        public MouseClickRawArgs(int x, int y, RawInputMouseBtn btn) : base(x, y)
        {
            Button = btn;
        }
    }

    public class KeyboardClickRawArgs : EventArgs
    {

    }

    public partial class RawInputDX : Window
    {
        #region setup        
        private IntPtr _progman, _workerW;
        private readonly ISettingsService _settings;
        private readonly IDesktopCore _desktopCore;

        public InputForwardMode InputMode { get; private set; }

        public event EventHandler<MouseRawArgs> MouseMoveRaw;
        public event EventHandler<MouseClickRawArgs> MouseDownRaw;
        public event EventHandler<MouseClickRawArgs> MouseUpRaw;
        public event EventHandler<KeyboardClickRawArgs> KeyboardClickRaw;


        public RawInputDX(ISettingsService settingsService, IDesktopCore desktopCore)
        {
            _settings = settingsService;
            _desktopCore = desktopCore;

            InitializeComponent();

            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = -99999;
            InputMode = InputForwardMode.MouseKeyboard;
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            var windowInteropHelper = new WindowInteropHelper(this);
            var hwnd = windowInteropHelper.Handle;

            switch (InputMode)
            {
                case InputForwardMode.Off:
                    Close();
                    break;
                case InputForwardMode.Mouse:
                    //ExInputSink flag makes it work even when not in foreground, similar to global hook.. but asynchronous, no complications and no AV false detection!
                    RawInputDevice.RegisterDevice(HidUsageAndPage.Mouse,
                        RawInputDeviceFlags.ExInputSink, hwnd);
                    break;
                case InputForwardMode.MouseKeyboard:
                    RawInputDevice.RegisterDevice(HidUsageAndPage.Mouse,
                        RawInputDeviceFlags.ExInputSink, hwnd);
                    RawInputDevice.RegisterDevice(HidUsageAndPage.Keyboard,
                        RawInputDeviceFlags.ExInputSink, hwnd);
                    break;
            }

            HwndSource source = HwndSource.FromHwnd(hwnd);
            source.AddHook(Hook);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            switch (InputMode)
            {
                case InputForwardMode.Off:
                    break;
                case InputForwardMode.Mouse:
                    RawInputDevice.UnregisterDevice(HidUsageAndPage.Mouse);
                    break;
                case InputForwardMode.MouseKeyboard:
                    RawInputDevice.UnregisterDevice(HidUsageAndPage.Mouse);
                    RawInputDevice.UnregisterDevice(HidUsageAndPage.Keyboard);
                    break;
            }
        }

        #endregion //setup

        #region input forward

        protected IntPtr Hook(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            if (msg == (int)Win32.WM.INPUT)
            {
                var data = RawInputData.FromHandle(lparam);

                switch (data)
                {
                    case RawInputMouseData mouse:
                        var M = System.Windows.Forms.Control.MousePosition;
                        switch (mouse.Mouse.Buttons)
                        {
                            case Linearstar.Windows.RawInput.Native.RawMouseButtonFlags.LeftButtonDown:
                                ForwardMessageMouse(M.X, M.Y, (int)Win32.WM.LBUTTONDOWN, (IntPtr)0x0001);
                                MouseDownRaw?.Invoke(this, new MouseClickRawArgs(M.X, M.Y, RawInputMouseBtn.left));
                                break;
                            case Linearstar.Windows.RawInput.Native.RawMouseButtonFlags.LeftButtonUp:
                                ForwardMessageMouse(M.X, M.Y, (int)Win32.WM.LBUTTONUP, (IntPtr)0x0001);
                                MouseUpRaw?.Invoke(this, new MouseClickRawArgs(M.X, M.Y, RawInputMouseBtn.left));
                                break;
                            case Linearstar.Windows.RawInput.Native.RawMouseButtonFlags.RightButtonDown:
                                MouseDownRaw?.Invoke(this, new MouseClickRawArgs(M.X, M.Y, RawInputMouseBtn.right));
                                break;
                            case Linearstar.Windows.RawInput.Native.RawMouseButtonFlags.RightButtonUp:
                                MouseUpRaw?.Invoke(this, new MouseClickRawArgs(M.X, M.Y, RawInputMouseBtn.right));
                                break;
                            case Linearstar.Windows.RawInput.Native.RawMouseButtonFlags.None:
                                ForwardMessageMouse(M.X, M.Y, (int)Win32.WM.MOUSEMOVE, (IntPtr)0x0020);
                                MouseMoveRaw?.Invoke(this, new MouseRawArgs(M.X, M.Y));
                                break;
                            case Linearstar.Windows.RawInput.Native.RawMouseButtonFlags.MouseWheel:
                                break;
                        }
                        break;
                    case RawInputKeyboardData keyboard:
                        ForwardMessageKeyboard((int)keyboard.Keyboard.WindowMessage,
                            (IntPtr)keyboard.Keyboard.VirutalKey, keyboard.Keyboard.ScanCode,
                            (keyboard.Keyboard.Flags != Linearstar.Windows.RawInput.Native.RawKeyboardFlags.Up));
                        KeyboardClickRaw?.Invoke(this, new KeyboardClickRawArgs());
                        break;
                }
            }
            return IntPtr.Zero;
        }

        private void ForwardMessageKeyboard(int msg, IntPtr wParam, int scanCode, bool isPressed)
        {
            try
            {
                if (_settings.Settings.InputForward == InputForwardMode.MouseKeyboard && IsDesktop())
                {
                    var monitor = new PoseidonMonitor(MonitorManager.Instance.GetMonitorFromPoint(new Point(
                        System.Windows.Forms.Control.MousePosition.X, System.Windows.Forms.Control.MousePosition.Y)));

                    foreach (var wallpaper in _desktopCore.Wallpapers)
                    {
                        if (IsInputAllowed(wallpaper.WallpaperType))
                        {
                            if (MonitorHelper.Compare(monitor, wallpaper.Monitor, MonitorIdentificationMode.DeviceId) ||
                                _settings.Settings.WallpaperArrangement == WallpaperArrangement.Span)
                            {
                                var lParam = 1u;
                                lParam |= (uint)scanCode << 16;
                                lParam |= 1u << 24;
                                lParam |= 0u << 29;

                                lParam = isPressed ? lParam : (lParam |= 3u << 30);
                                Win32.PostMessageW(wallpaper.InputHandle, msg, wParam, (UIntPtr)lParam);
                            }
                        }
                    }
                }
            }
            catch
            {

            }
        }

        private void ForwardMessageMouse(int x, int y, int msg, IntPtr wParam)
        {
            if (_settings.Settings.InputForward == InputForwardMode.Off)
            {
                return;
            }
            else if (!IsDesktop())
            {
                if (msg != (int)Win32.WM.MOUSEMOVE || !_settings.Settings.MouseInputMoveAlways)
                {
                    return;
                }
            }

            try
            {
                var monitor = new PoseidonMonitor(MonitorManager.Instance.GetMonitorFromPoint(new Point(x, y)));
                var mouse = CalculateMousePos(x, y, monitor, _settings.Settings.WallpaperArrangement);

                foreach (var wallpaper in _desktopCore.Wallpapers)
                {
                    if (IsInputAllowed(wallpaper.WallpaperType))
                    {
                        if (MonitorHelper.Compare(monitor, wallpaper.Monitor, MonitorIdentificationMode.DeviceId) ||
                            _settings.Settings.WallpaperArrangement == WallpaperArrangement.Span)
                        {
                            var lParam = Convert.ToUInt32(mouse.Y);
                            lParam <<= 16;
                            lParam |= Convert.ToUInt32(mouse.X);
                            Win32.PostMessageW(wallpaper.InputHandle, msg, wParam, (UIntPtr)lParam);
                        }
                    }
                }
            }
            catch
            {

            }
        }

        #endregion //input forward

        #region helpers

        private static Point CalculateMousePos(int x, int y, PoseidonMonitor monitor, WallpaperArrangement arrangement)
        {
            if (MonitorHelper.IsMultiMonitor())
            {
                if (arrangement == WallpaperArrangement.Span)
                {
                    var MonitorArea = MonitorHelper.GetVirtualMonitorBounds();
                    x -= MonitorArea.Location.X;
                    y -= MonitorArea.Location.Y;
                }
                else
                {
                    x += -1 * monitor.Bounds.X;
                    y += -1 * monitor.Bounds.Y;
                }
            }
            return new Point(x, y);
        }

        private static bool IsInputAllowed(WallpaperType type)
        {
            return type switch
            {
                WallpaperType.App => true,
                WallpaperType.Web => true,
                WallpaperType.WebAudio => true,
                WallpaperType.Url => true,
                WallpaperType.Bizhawk => true,
                WallpaperType.Unity => true,
                WallpaperType.Godot => true,
                WallpaperType.Video => false,
                WallpaperType.Gif => false,
                WallpaperType.UnityAudio => true,
                WallpaperType.VideoStream => false,
                WallpaperType.Picture => false,
                _ => false,
            };
        }

        private bool IsDesktop()
        {
            IntPtr hWnd = Win32.GetForegroundWindow();
            return (Equals(hWnd, _workerW) || Equals(hWnd, _progman));
        }

        #endregion //helpers
    }
}
