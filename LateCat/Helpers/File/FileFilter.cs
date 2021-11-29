using LateCat.PoseidonEngine.Core;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace LateCat.Helpers
{
    public class FileFilter
    {
        public static readonly FileData[] SupportedFormats = new FileData[] {
            new FileData(WallpaperType.Video, new string[]{".wmv", ".avi", ".flv", ".m4v",
                    ".mkv", ".mov", ".mp4", ".mp4v", ".mpeg4",
                    ".mpg", ".webm", ".ogm", ".ogv", ".ogx" }),
            new FileData(WallpaperType.Picture, new string[] {".jpg", ".jpeg", ".png",
                    ".bmp", ".tif", ".tiff", ".webp" }),
            new FileData(WallpaperType.Gif, new string[]{".gif" }),
            //new FileData(WallpaperType.heic, new string[] {".heic" }),//, ".heics", ".heif", ".heifs" }),
            new FileData(WallpaperType.Web, new string[]{".html" }),
            new FileData(WallpaperType.WebAudio, new string[]{".html" }),
            new FileData(WallpaperType.App, new string[]{".exe" }),
            //new FileFilter(WallpaperType.unity,"*.exe"),
            //new FileFilter(WallpaperType.unityaudio,"Unity Audio Visualiser |*.exe"),
            new FileData(WallpaperType.Godot, new string[]{".exe" }),
            
            new FileData((WallpaperType)(100),  new string[]{".zip" })
        };

        public static string GetLocalisedWallpaperTypeString(WallpaperType type)
        {
            string localisedText = type switch
            {
                WallpaperType.App => Properties.Resources.TextApplication,
                WallpaperType.Unity => "Unity",
                WallpaperType.Godot => "Godot",
                WallpaperType.UnityAudio => "Unity",
                WallpaperType.Bizhawk => "Bizhawk",
                WallpaperType.Web => Properties.Resources.TextWebsite,
                WallpaperType.WebAudio => Properties.Resources.TitleAudio,
                WallpaperType.Url => Properties.Resources.TextWebsite,
                WallpaperType.Video => Properties.Resources.TextVideo,
                WallpaperType.Gif => "Gif",
                WallpaperType.VideoStream => Properties.Resources.TextWebStream,
                WallpaperType.Picture => Properties.Resources.TextPicture,
                //WallpaperType.heic => "HEIC",
                (WallpaperType)(100) => Properties.Resources.TitleAppName,
                _ => Properties.Resources.TextError,
            };
            return localisedText;
        }
        
        public static WallpaperType GetFileType(string filePath)
        {           
            var item = SupportedFormats.FirstOrDefault(
                x => x.Extentions.Any(y => y.Equals(Path.GetExtension(filePath), StringComparison.OrdinalIgnoreCase)));

            return item != null ? item.Type : (WallpaperType)(-1);
        }
        
        public static string GetSupportedFileDialogFilter(bool anyFile = false)
        {
            var filterString = new StringBuilder();

            if (anyFile)
            {
                filterString.Append(Properties.Resources.TextAllFiles + "|*.*|");
            }
            foreach (var item in SupportedFormats)
            {
                filterString.Append(GetLocalisedWallpaperTypeString(item.Type));
                filterString.Append("|");
                foreach (var extension in item.Extentions)
                {
                    filterString.Append("*").Append(extension).Append(";");
                }
                filterString.Remove(filterString.Length - 1, 1);
                filterString.Append("|");
            }
            filterString.Remove(filterString.Length - 1, 1);

            return filterString.ToString();
        }
    }
}
