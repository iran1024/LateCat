namespace LateCat.PoseidonEngine.Abstractions
{
    internal interface IWallpaperFactory
    {
        IWallpaper CreateWallpaper(IWallpaperMetadata metadata, IMonitor monitor, ISettingsService settings);
    }
}