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
            //Starting a hidden window outside Monitor region, rawinput receives msg through WndProc
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
            // You can read inputs by processing the WM_INPUT message.
            if (msg == (int)Win32.WM.INPUT)
            {
                // Create an RawInputData from the handle stored in lParam.
                var data = RawInputData.FromHandle(lparam);

                //You can identify the source device using Header.DeviceHandle or just Device.
                //var sourceDeviceHandle = data.Header.DeviceHandle;
                //var sourceDevice = data.Device;

                // The data will be an instance of either RawInputMouseData, RawInputKeyboardData, or RawInputHidData.
                // They contain the raw input data in their properties.
                switch (data)
                {
                    case RawInputMouseData mouse:
                        //RawInput only gives relative mouse movement value.. cheating here with Winform library.
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
                                //issue: click being skipped; desktop already has its own rightclick contextmenu.
                                //ForwardMessage(M.X, M.Y, (int)Win32.WM.RBUTTONDOWN, (IntPtr)0x0002);
                                MouseDownRaw?.Invoke(this, new MouseClickRawArgs(M.X, M.Y, RawInputMouseBtn.right));
                                break;
                            case Linearstar.Windows.RawInput.Native.RawMouseButtonFlags.RightButtonUp:
                                //issue: click being skipped; desktop already has its own rightclick contextmenu.
                                //ForwardMessage(M.X, M.Y, (int)Win32.WM.RBUTTONUP, (IntPtr)0x0002);
                                MouseUpRaw?.Invoke(this, new MouseClickRawArgs(M.X, M.Y, RawInputMouseBtn.right));
                                break;
                            case Linearstar.Windows.RawInput.Native.RawMouseButtonFlags.None:
                                ForwardMessageMouse(M.X, M.Y, (int)Win32.WM.MOUSEMOVE, (IntPtr)0x0020);
                                MouseMoveRaw?.Invoke(this, new MouseRawArgs(M.X, M.Y));
                                break;
                            case Linearstar.Windows.RawInput.Native.RawMouseButtonFlags.MouseWheel:
                                //Disabled, not tested yet.
                                /*
                                https://github.com/ivarboms/game-engine/blob/master/Input/RawInput.cpp
                                Mouse wheel deltas are represented as multiples of 120.
                                MSDN: The delta was set to 120 to allow Microsoft or other vendors to build
                                finer-resolution wheels (a freely-rotating wheel with no notches) to send more
                                messages per rotation, but with a smaller value in each message.
                                Because of this, the value is converted to a float in case a mouse's wheel
                                reports a value other than 120, in which case dividing by 120 would produce
                                a very incorrect value.
                                More info: http://social.msdn.microsoft.com/forums/en-US/gametechnologiesgeneral/thread/1deb5f7e-95ee-40ac-84db-58d636f601c7/
                                */

                                /*
                                // One wheel notch is represented as this delta (WHEEL_DELTA).
                                const float oneNotch = 120;

                                // Mouse wheel delta in multiples of WHEEL_DELTA (120).
                                float mouseWheelDelta = mouse.Mouse.RawButtons;

                                // Convert each notch from [-120, 120] to [-1, 1].
                                mouseWheelDelta = mouseWheelDelta / oneNotch;

                                MouseScrollSimulate(mouseWheelDelta);
                                */
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

        /// <summary>
        /// Forwards the keyboard message to the required wallpaper window based on given cursor location.<br/>
        /// Skips if desktop is not focused.
        /// </summary>
        /// <param name="msg">key press msg.</param>
        /// <param name="wParam">Virtual-Key code.</param>
        /// <param name="scanCode">OEM code of the key.</param>
        /// <param name="isPressed">Key is pressed.</param>
        private void ForwardMessageKeyboard(int msg, IntPtr wParam, int scanCode, bool isPressed)
        {
            try
            {
                //Don't forward when not on desktop.
                if (_settings.Settings.InputForward == InputForwardMode.MouseKeyboard && IsDesktop())
                {
                    //Detect active wp based on cursor pos, better way to do this?
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
                //Logger.Error("Keyboard Forwarding Error:" + e.Message);
            }
        }

        /// <summary>
        /// Forwards the mouse message to the required wallpaper window based on given cursor location.<br/>
        /// Skips if apps are in foreground.
        /// </summary>
        /// <param name="x">Cursor pos x</param>
        /// <param name="y">Cursor pos y</param>
        /// <param name="msg">mouse message</param>
        /// <param name="wParam">additional msg parameter</param>
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
                            uint lParam = Convert.ToUInt32(mouse.Y);
                            lParam <<= 16;
                            lParam |= Convert.ToUInt32(mouse.X);
                            Win32.PostMessageW(wallpaper.InputHandle, msg, wParam, (UIntPtr)lParam);
                        }
                    }
                }
            }
            catch
            {
                //Logger.Error("Mouse Forwarding Error:" + e.Message);
            }
        }

        #endregion //input forward

        #region helpers

        private Point CalculateMousePos(int x, int y, PoseidonMonitor monitor, WallpaperArrangement arrangement)
        {
            if (MonitorHelper.IsMultiMonitor())
            {
                if (arrangement == WallpaperArrangement.Span)
                {
                    var MonitorArea = MonitorHelper.GetVirtualMonitorBounds();
                    x -= MonitorArea.Location.X;
                    y -= MonitorArea.Location.Y;
                }
                else //per-display or duplicate mode.
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
            return (IntPtr.Equals(hWnd, _workerW) || IntPtr.Equals(hWnd, _progman));
        }

        #endregion //helpers
    }
}
