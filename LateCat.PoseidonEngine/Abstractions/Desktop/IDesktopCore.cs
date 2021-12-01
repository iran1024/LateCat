using LateCat.PoseidonEngine.Core;
using System;
using System.Collections.ObjectModel;

namespace LateCat.PoseidonEngine.Abstractions
{
    public interface IDesktopCore : IDisposable
    {
        IntPtr WorkerW { get; }
        ReadOnlyCollection<IWallpaper> Wallpapers { get; }
        void CloseAllWallpapers(bool terminate = false);
        void CloseWallpaper(IWallpaperMetadata metadata, bool terminate = false);
        void CloseWallpaper(IMonitor monitor, bool terminate = false);
        void CloseWallpaper(WallpaperType type, bool terminate = false);
        void ResetWallpaper();
        void RestoreWallpaper();
        void SeekWallpaper(IWallpaperMetadata wp, float seek, PlaybackPosType type);
        void SeekWallpaper(IMonitor monitor, float seek, PlaybackPosType type);
        void SetWallpaper(IWallpaperMetadata wallpaper, IMonitor display);

        void SendMessage(IWallpaperMetadata metadata, IPCMessage message);
        void SendMessage(IMonitor monitor, IWallpaperMetadata metadata, IPCMessage message);

        public event EventHandler WallpaperChanged;

        public event EventHandler WallpaperReset;
    }
}
