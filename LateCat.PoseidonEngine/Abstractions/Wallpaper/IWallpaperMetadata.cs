using LateCat.PoseidonEngine.Core;
using LateCat.PoseidonEngine.Models;
using System;

namespace LateCat.PoseidonEngine.Abstractions
{
    public interface IWallpaperMetadata
    {
        string Author { get; set; }
        WallpaperProcessStatus Status { get; set; }
        string Desc { get; set; }
        string FilePath { get; set; }
        bool ItemStartup { get; set; }
        WallpaperInfo WallpaperInfo { get; set; }
        string InfoFolderPath { get; set; }
        string PropertyPath { get; set; }
        Uri SrcWebsite { get; set; }
        string Title { get; set; }
        string WallpaperType { get; set; }
    }
}
