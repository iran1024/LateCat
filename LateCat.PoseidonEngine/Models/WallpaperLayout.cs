using LateCat.PoseidonEngine.Abstractions;
using LateCat.PoseidonEngine.Core;
using Newtonsoft.Json;
using System;
using System.Drawing;

namespace LateCat.PoseidonEngine.Models
{
    [Serializable]
    public class WallpaperLayout : IWallpaperLayout
    {
        public string InfoPath { get; set; }

        public PoseidonMonitor Monitor { get; set; }

        [JsonConstructor]
        public WallpaperLayout(string DeviceId, string DeviceName, int BitsPerPixel, Rectangle Bounds, Rectangle WorkingArea, string infoPath)
        {
            Monitor = new PoseidonMonitor(DeviceId, DeviceName, BitsPerPixel, Bounds, WorkingArea);
            InfoPath = infoPath;
        }

        public WallpaperLayout(PoseidonMonitor monitor, string infoPath)
        {
            Monitor = monitor;
            InfoPath = infoPath;
        }
    }
}
