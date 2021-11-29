using LateCat.PoseidonEngine.Core;

namespace LateCat.PoseidonEngine.Models
{
    public class WallpaperAggregation
    {
        public WallpaperDetail Instance { get; set; }

        public int Index { get; set; }

        public IndexState IndexState { get; set; }

        public WallpaperAggregation(WallpaperDetail detail, int index, IndexState indexState = IndexState.Normal)
        {
            Instance = detail;
            Index = index;
            IndexState = indexState;
        }
    }
}