using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;

namespace LateCat.PoseidonEngine.Core
{
    public class MonitorManager : ObservableObject
    {
        private const int PRIMARY_MONITOR = unchecked((int)0xBAADF00D);

        private const int MONITORINFOF_PRIMARY = 0x00000001;
        private const int MONITOR_DEFAULTTONEAREST = 0x00000002;

        private static bool _multiMonitorSupport;
        private const string _defaultMonitorDeviceName = "DISPLAY";

        public static MonitorManager Instance { get; private set; }

        public event EventHandler MonitorUpdated;

        public ObservableCollection<SystemMonitor> Monitors { get; } = new ObservableCollection<SystemMonitor>();

        private Rect _virtualMonitorBounds = Rect.Empty;

        public Rect VirtualMonitorBounds
        {
            get => _virtualMonitorBounds;
            private set => SetProperty(ref _virtualMonitorBounds, value);
        }

        public SystemMonitor PrimaryMonitor => Monitors
            .FirstOrDefault(x => x.IsPrimary);

        private MonitorManager()
        {
            RefreshMonitorList();
        }

        public static void Initialize()
        {
            Instance = new MonitorManager();
        }

        public uint OnHwndCreated(IntPtr hWnd, out bool register)
        {
            register = false;
            return 0;
        }

        public IntPtr OnWndProc(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == (uint)Win32.WM.DISPLAYCHANGE)
            {
                RefreshMonitorList();
            }
            return IntPtr.Zero;
        }

        public SystemMonitor GetMonitorFromHWnd(IntPtr hWnd)
        {
            IntPtr hMonitor = _multiMonitorSupport
                ? Win32.MonitorFromWindow(new HandleRef(null, hWnd), MONITOR_DEFAULTTONEAREST)
                : (IntPtr)PRIMARY_MONITOR;

            return GetMonitorFromHMonitor(hMonitor);
        }

        public SystemMonitor GetMonitorFromPoint(Point point)
        {
            IntPtr hMonitor;
            if (_multiMonitorSupport)
            {
                var pt = new Win32.POINT(
                    (int)Math.Round(point.X),
                    (int)Math.Round(point.Y));
                hMonitor = Win32.MonitorFromPoint(pt, MONITOR_DEFAULTTONEAREST);
            }
            else
                hMonitor = (IntPtr)PRIMARY_MONITOR;

            return GetMonitorFromHMonitor(hMonitor);
        }

        private void RefreshMonitorList()
        {
            _multiMonitorSupport = Win32.GetSystemMetrics((int)Win32.SystemMetric.SM_CMONITORS) != 0;

            var hMonitors = GetHMonitors();

            foreach (var displayMonitor in Monitors)
            {
                displayMonitor._isStale = true;
            }

            for (int i = 0; i < hMonitors.Count; i++)
            {
                var displayMonitor = GetMonitorFromHMonitor(hMonitors[i]);
                displayMonitor.Index = i + 1;
            }

            var staleMonitors = Monitors
                .Where(x => x._isStale).ToList();
            foreach (var displayMonitor in staleMonitors)
            {
                Monitors.Remove(displayMonitor);
            }

            staleMonitors.Clear();
            staleMonitors = null;

            VirtualMonitorBounds = GetVirtualMonitorBounds();

            MonitorUpdated?.Invoke(this, EventArgs.Empty);
        }

        private SystemMonitor GetMonitorFromHMonitor(IntPtr hMonitor)
        {
            SystemMonitor displayMonitor = null;

            if (!_multiMonitorSupport || hMonitor == (IntPtr)PRIMARY_MONITOR)
            {
                displayMonitor = GetMonitorByDeviceName(_defaultMonitorDeviceName);

                if (displayMonitor == null)
                {
                    displayMonitor = new SystemMonitor(_defaultMonitorDeviceName);
                    Monitors.Add(displayMonitor);
                }

                displayMonitor.Bounds = GetVirtualMonitorBounds();
                displayMonitor.DeviceId = GetDefaultMonitorDeviceId();
                displayMonitor.MonitorName = "Display";
                displayMonitor.HMonitor = hMonitor;
                displayMonitor.IsPrimary = true;
                displayMonitor.WorkingArea = GetWorkingArea();

                displayMonitor._isStale = false;
            }
            else
            {
                var info = new Win32.MONITORINFOEX();// MONITORINFOEX();
                Win32.GetMonitorInfo(new HandleRef(null, hMonitor), info);

                string deviceName = new string(info.szDevice).TrimEnd((char)0);

                displayMonitor = GetMonitorByDeviceName(deviceName);

                displayMonitor ??= CreateMonitorFromMonitorInfo(deviceName);

                displayMonitor.HMonitor = hMonitor;

                UpdateMonitor(displayMonitor, info);
            }

            return displayMonitor;
        }

        private SystemMonitor GetMonitorByDeviceName(string deviceName)
        {
            return Monitors.FirstOrDefault(x => x.DeviceName == deviceName);
        }

        private SystemMonitor CreateMonitorFromMonitorInfo(string deviceName)
        {
            var displayMonitor = new SystemMonitor(deviceName);

            var displayDevice = GetMonitorDevice(deviceName);
            displayMonitor.DeviceId = displayDevice.DeviceID;
            displayMonitor.MonitorName = displayDevice.DeviceString;

            Monitors.Add(displayMonitor);

            return displayMonitor;
        }

        private void UpdateMonitor(SystemMonitor displayMonitor, Win32.MONITORINFOEX info)
        {
            displayMonitor.Bounds = new Rect(
                info.rcMonitor.Left, info.rcMonitor.Top,
                info.rcMonitor.Right - info.rcMonitor.Left,
                info.rcMonitor.Bottom - info.rcMonitor.Top);

            displayMonitor.IsPrimary = (info.dwFlags & MONITORINFOF_PRIMARY) != 0;

            displayMonitor.WorkingArea = new Rect(
                info.rcWork.Left, info.rcWork.Top,
                info.rcWork.Right - info.rcWork.Left,
                info.rcWork.Bottom - info.rcWork.Top);

            displayMonitor._isStale = false;
        }

        private IList<IntPtr> GetHMonitors()
        {
            if (_multiMonitorSupport)
            {
                var hMonitors = new List<IntPtr>();

                bool callback(IntPtr monitor, IntPtr hdc, IntPtr lprcMonitor, IntPtr lParam)
                {
                    hMonitors.Add(monitor);
                    return true;
                }

                Win32.EnumDisplayMonitors(new HandleRef(null, IntPtr.Zero), null, callback, IntPtr.Zero);

                return hMonitors;
            }

            return new[] { (IntPtr)PRIMARY_MONITOR };
        }

        private static Win32.DISPLAY_DEVICE GetMonitorDevice(string deviceName)
        {
            var result = new Win32.DISPLAY_DEVICE();

            var displayDevice = new Win32.DISPLAY_DEVICE();
            displayDevice.cb = Marshal.SizeOf(displayDevice);
            try
            {
                for (uint id = 0; Win32.EnumDisplayDevices(deviceName, id, ref displayDevice, Win32.EDD_GET_DEVICE_INTERFACE_NAME); id++)
                {
                    if (displayDevice.StateFlags.HasFlag(Win32.DisplayDeviceStateFlags.AttachedToDesktop)
                        && !displayDevice.StateFlags.HasFlag(Win32.DisplayDeviceStateFlags.MirroringDriver))
                    {
                        result = displayDevice;
                        break;
                    }

                    displayDevice.cb = Marshal.SizeOf(displayDevice);
                }
            }
            catch { }

            if (string.IsNullOrEmpty(result.DeviceID)
                || string.IsNullOrWhiteSpace(result.DeviceID))
            {
                result.DeviceID = GetDefaultMonitorDeviceId();
            }

            return result;
        }

        private static string GetDefaultMonitorDeviceId() => Win32.GetSystemMetrics((int)Win32.SystemMetric.SM_REMOTESESSION) != 0 ?
                    "\\\\?\\MONITOR#REMOTEMONITOR#" : "\\\\?\\MONITOR#LOCALMONITOR#";

        private static Rect GetVirtualMonitorBounds()
        {
            var location = new Point(Win32.GetSystemMetrics(
                (int)Win32.SystemMetric.SM_XVIRTUALSCREEN), Win32.GetSystemMetrics((int)Win32.SystemMetric.SM_YVIRTUALSCREEN));
            var size = new Size(Win32.GetSystemMetrics(
                (int)Win32.SystemMetric.SM_CXVIRTUALSCREEN), Win32.GetSystemMetrics((int)Win32.SystemMetric.SM_CYVIRTUALSCREEN));
            return new Rect(location, size);
        }

        private static Rect GetWorkingArea()
        {
            var rc = new Win32.RECT();
            Win32.SystemParametersInfo((int)Win32.SPI.SPI_GETWORKAREA, 0, ref rc, 0);
            return new Rect(rc.Left, rc.Top,
                rc.Right - rc.Left, rc.Bottom - rc.Top);
        }
    }
}