using System;

namespace LateCat.PoseidonEngine.Abstractions
{
    public interface IWallpaperMonitor : IDisposable
    {
        void Start();

        void Stop();
    }
}