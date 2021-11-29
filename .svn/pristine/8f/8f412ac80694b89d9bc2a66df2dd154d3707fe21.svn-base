using LateCat.PoseidonEngine.Abstractions;
using LateCat.PoseidonEngine.Core;
using System;
using System.Reflection;

namespace LateCat.PoseidonEngine.Models
{
    [Serializable]
    public class WallpaperInfo : IWallpaperInfo
    {
        public string AppVersion { get; set; }
        public string Title { get; set; }
        public string Desc { get; set; }
        public string Author { get; set; }
        public string License { get; set; }
        public string Contact { get; set; }
        public WallpaperType Type { get; set; }
        public string FileName { get; set; }
        public string Arguments { get; set; }
        public bool IsAbsolutePath { get; set; }

        public WallpaperInfo()
        {
            AppVersion = Assembly.GetExecutingAssembly().GetName().Version!.ToString();
            Title = string.Empty;
            Type = WallpaperType.Web;
            FileName = string.Empty;
            Desc = string.Empty;
            Author = string.Empty;
            License = string.Empty;
            Contact = string.Empty;
            Arguments = string.Empty;
            IsAbsolutePath = false;
        }

        public WallpaperInfo(WallpaperInfo info)
        {
            AppVersion = Assembly.GetExecutingAssembly().GetName().Version!.ToString();
            Title = info.Title;
            Type = info.Type;
            FileName = info.FileName;
            Desc = info.Desc;
            Author = info.Author;
            Contact = info.Contact;
            License = info.License;
            Arguments = info.Arguments;
            IsAbsolutePath = info.IsAbsolutePath;
        }
    }
}
