using LateCat.PoseidonEngine.Core;

namespace LateCat.PoseidonEngine.Abstractions
{
    public interface IWallpaperLayout
    {
        public string InfoPath { get; }

        public PoseidonMonitor Monitor { get; }
    }
}
