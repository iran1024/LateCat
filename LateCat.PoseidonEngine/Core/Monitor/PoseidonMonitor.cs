using LateCat.PoseidonEngine.Abstractions;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Linq;

namespace LateCat.PoseidonEngine.Core
{
    [Serializable]
    public class PoseidonMonitor : IMonitor
    {
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string DeviceNumber { get; set; }
        public int BitsPerPixel { get; set; }
        public Rectangle Bounds { get; set; }
        public Rectangle WorkingArea { get; set; }

        [JsonConstructor]
        public PoseidonMonitor(string deviceId, string deviceName, int bitsPerPixel, Rectangle bounds, Rectangle workingArea)
        {
            DeviceId = deviceId ?? (MonitorHelper.GetMonitor().FirstOrDefault(x => x.Bounds == bounds)?.DeviceId);
            DeviceName = deviceName;
            DeviceNumber = MonitorHelper.GetMonitorNumber(deviceName);
            BitsPerPixel = bitsPerPixel;
            Bounds = bounds;
            WorkingArea = workingArea;
        }

        public PoseidonMonitor(SystemMonitor monitor)
        {
            DeviceId = monitor.DeviceId;
            DeviceName = monitor.DeviceName;
            DeviceNumber = monitor.Index.ToString();
            BitsPerPixel = 0;
            Bounds = MonitorHelper.RectToRectangle(monitor.Bounds);
            WorkingArea = MonitorHelper.RectToRectangle(monitor.WorkingArea);
        }

        public PoseidonMonitor(System.Windows.Forms.Screen monitor)
        {
            DeviceId = MonitorHelper.GetMonitor().FirstOrDefault(x => x.Bounds == Bounds)?.DeviceId;
            DeviceName = monitor.DeviceName;
            DeviceNumber = MonitorHelper.GetMonitorNumber(monitor.DeviceName);
            BitsPerPixel = monitor.BitsPerPixel;
            Bounds = monitor.Bounds;
            WorkingArea = monitor.WorkingArea;
        }

        public bool Equals(IMonitor monitor)
        {
            return DeviceId == monitor.DeviceId;
        }
    }
}