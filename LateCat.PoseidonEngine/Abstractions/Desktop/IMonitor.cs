using System;
using System.Drawing;

namespace LateCat.PoseidonEngine.Abstractions
{
    public interface IMonitor : IEquatable<IMonitor>
    {
        int BitsPerPixel { get; set; }
        Rectangle Bounds { get; set; }
        string DeviceId { get; set; }
        string DeviceName { get; set; }
        string DeviceNumber { get; set; }
        Rectangle WorkingArea { get; set; }
    }
}
