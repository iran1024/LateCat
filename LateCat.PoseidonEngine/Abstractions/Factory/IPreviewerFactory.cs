namespace LateCat.PoseidonEngine.Abstractions
{
    public interface IPreviewerFactory
    {
        IPreviewer Get<TPreviewer>(IWallpaperMetadata metadata) where TPreviewer : IPreviewer;
    }
}