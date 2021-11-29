using LateCat.PoseidonEngine.Models;

namespace LateCat.PoseidonEngine.Abstractions
{
    internal interface IWallpaperGetter
    {
        void Initialize();

        void Reload(int current);

        WallpaperAggregation Previous();

        WallpaperAggregation Next();

        WallpaperAggregation Default();

        WallpaperAggregation Current();

        bool Prepared();

        bool Reloaded();

        string GetWallpaperApiEndpoint();

        bool SetWallpaperApiEndpoint(string endpoint);

        bool VerifyWallpaperApiEndpoint(string endpoint);
    }
}
