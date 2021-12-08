namespace LateCat.PoseidonEngine.Abstractions
{
    public interface IPreviewer
    {
        IPreviewer GetPreviewer(IWallpaperMetadata metadata);

        IPreviewer ChangeSource(IWallpaperMetadata metadata);

        void Preview();

        void Close();

        IWallpaperMetadata Metadata { get; }
    }
}