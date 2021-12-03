namespace LateCat.PoseidonEngine.Abstractions
{
    public interface IPreviewer
    {
        IPreviewer GetPreviewer(IWallpaperMetadata metadata);

        IPreviewer ChangeSource(IWallpaperMetadata metadata);

        virtual void Preview() { }

        IWallpaperMetadata Metadata { get; }
    }
}