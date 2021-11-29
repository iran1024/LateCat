using LateCat.PoseidonEngine.Abstractions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace LateCat.PoseidonEngine.Core
{
    public static class MonitorHelper
    {
        public static event EventHandler MonitorUpdated;
        private static readonly List<IMonitor> _monitors = new();

        public static void Initialize()
        {
            MonitorManager.Initialize();
            UpdateMonitorList();
            MonitorManager.Instance.MonitorUpdated += Instance_MonitorUpdated;
        }

        private static void Instance_MonitorUpdated(object sender, System.EventArgs e)
        {
            UpdateMonitorList();
            MonitorUpdated?.Invoke(null, EventArgs.Empty);
        }

        public static List<IMonitor> GetMonitor()
        {
            return _monitors;
        }

        public static bool IsMultiMonitor()
        {
            return MonitorManager.Instance.Monitors.Count > 1;
        }

        public static int MonitorCount()
        {
            return MonitorManager.Instance.Monitors.Count;
        }

        public static IMonitor GetPrimaryMonitor()
        {
            return new PoseidonMonitor(MonitorManager.Instance.PrimaryMonitor);
        }

        public static bool Exists(IMonitor monitor, MonitorIdentificationMode mode)
        {
            bool status = false;
            switch (mode)
            {
                case MonitorIdentificationMode.DeviceName:
                    foreach (var item in Screen.AllScreens)
                    {
                        if (item.DeviceName == monitor.DeviceName)
                        {
                            status = true;
                            break;
                        }
                    }
                    break;
                case MonitorIdentificationMode.DeviceId:
                    status = GetMonitor().FirstOrDefault(x => x.DeviceId == monitor.DeviceId) != null;
                    break;
                case MonitorIdentificationMode.MonitorLayout:
                    status = GetMonitor().FirstOrDefault(x => x.Bounds == monitor.Bounds) != null;
                    break;
            }
            return status;
        }

        public static bool Compare(IMonitor monitor1, IMonitor monitor2, MonitorIdentificationMode mode)
        {
            bool screenStatus = false;
            switch (mode)
            {
                case MonitorIdentificationMode.DeviceName:
                    screenStatus = (monitor1.DeviceName == monitor2.DeviceName);
                    break;
                case MonitorIdentificationMode.DeviceId:
                    screenStatus = (monitor1.DeviceId == monitor2.DeviceId);
                    break;
                case MonitorIdentificationMode.MonitorLayout:
                    screenStatus = (monitor1.Bounds == monitor2.Bounds);
                    break;
            }
            return screenStatus;
        }

        public static IMonitor GetMonitor(string DeviceId, string DeviceName, Rectangle Bounds, Rectangle WorkingArea, MonitorIdentificationMode mode)
        {
            foreach (var item in GetMonitor())
            {
                switch (mode)
                {
                    case MonitorIdentificationMode.DeviceName:
                        if (item.DeviceName == DeviceName)
                        {
                            return item;
                        }
                        break;
                    case MonitorIdentificationMode.DeviceId:
                        if (item.DeviceId == DeviceId)
                        {
                            return item;
                        }
                        break;
                    case MonitorIdentificationMode.MonitorLayout:
                        if (item.Bounds == Bounds)
                        {
                            return item;
                        }
                        break;
                }
            }
            return null;
        }

        public static Rectangle GetVirtualMonitorBounds()
        {
            return RectToRectangle(MonitorManager.Instance.VirtualMonitorBounds);
        }

        public static Rectangle RectToRectangle(System.Windows.Rect rect)
        {
            return new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
        }

        private static void UpdateMonitorList()
        {
            _monitors.Clear();
            MonitorManager.Instance.Monitors.ToList().ForEach(
                screen => _monitors.Add(new PoseidonMonitor(screen)));
        }

        public static IMonitor GetMonitorFromPoint(Point pt)
        {
            return new PoseidonMonitor(
                MonitorManager.Instance.GetMonitorFromPoint(
                    new System.Windows.Point(pt.X, pt.Y)));
        }

        public static string GetMonitorNumber(string DeviceName)
        {
            if (DeviceName == null)
                return "-1";

            var result = Regex.Match(DeviceName, @"\d+$", RegexOptions.RightToLeft);
            return result.Success ? result.Value : "-1";
        }
    }
}