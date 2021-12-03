namespace LateCat.PoseidonEngine.Abstractions
{
    internal interface IPreviewerProvider
    {
        IPreviewer GetPreviewer(IWallpaperMetadata metadata);
    }
}
