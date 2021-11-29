using LateCat.PoseidonEngine.Abstractions;
using LateCat.PoseidonEngine.Core;
using Newtonsoft.Json;
using System;
using System.Drawing;

namespace LateCat.PoseidonEngine.Models
{
    [Serializable]
    internal class WallpaperLayout : IWallpaperLayout
    {
        public string ConfigPath { get; set; }

        public PoseidonMonitor Monitor { get; set; }

        [JsonConstructor]
        public WallpaperLayout(string DeviceId, string DeviceName, int BitsPerPixel, Rectangle Bounds, Rectangle WorkingArea, string infoPath)
        {
            Monitor = new PoseidonMonitor(DeviceId, DeviceName, BitsPerPixel, Bounds, WorkingArea);
            ConfigPath = infoPath;
        }

        public WallpaperLayout(PoseidonMonitor Display, string infoPath)
        {
            Monitor = Display;
            ConfigPath = infoPath;
        }
    }
}
