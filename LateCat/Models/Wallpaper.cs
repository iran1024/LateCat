﻿using LateCat.Common;
using LateCat.Helpers;
using LateCat.PoseidonEngine.Abstractions;
using LateCat.PoseidonEngine.Core;
using LateCat.PoseidonEngine.Models;
using LateCat.PoseidonEngine.Utilities;
using System;
using System.IO;

namespace LateCat.Models
{
    public class Wallpaper : ObservableObject, IWallpaperMetadata
    {
        private WallpaperInfo _wallpaperInfo;

        public WallpaperInfo WallpaperInfo
        {
            get
            {
                return _wallpaperInfo;
            }
            set
            {
                _wallpaperInfo = value;
                OnPropertyChanged();
            }
        }

        private WallpaperProcessStatus _status;
        public WallpaperProcessStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        private string _filePath;
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                if (WallpaperInfo.Type == PoseidonEngine.Core.WallpaperType.Url
                || WallpaperInfo.Type == PoseidonEngine.Core.WallpaperType.VideoStream)
                {
                    _filePath = value;
                }
                else
                {
                    _filePath = File.Exists(value) ? value : string.Empty;
                }
                OnPropertyChanged();
            }
        }

        private string _infoFolderPath;
        public string InfoFolderPath
        {
            get { return _infoFolderPath; }
            set
            {
                _infoFolderPath = value;
                OnPropertyChanged();
            }
        }

        private string _title;
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = string.IsNullOrWhiteSpace(value) ? "---" : (value.Length > 100 ? value[..100] : value);
                OnPropertyChanged();
            }
        }

        private string _author;
        public string Author
        {
            get
            {
                return _author;
            }
            set
            {
                _author = string.IsNullOrWhiteSpace(value) ? "---" : (value.Length > 100 ? value[..100] : value);
                OnPropertyChanged();
            }
        }

        private string _desc;
        public string Desc
        {
            get
            {
                return _desc;
            }
            set
            {
                _desc = string.IsNullOrWhiteSpace(value) ? WallpaperType : (value.Length > 5000 ? value[..5000] : value);
                OnPropertyChanged();
            }
        }

        private Uri _srcWebsite;
        public Uri SrcWebsite
        {
            get
            {
                return _srcWebsite;
            }
            set
            {
                _srcWebsite = value;
                OnPropertyChanged();
            }
        }

        private string _wallpaperType;

        public string WallpaperType
        {
            get
            {
                return _wallpaperType;
            }
            set
            {
                _wallpaperType = value;
                OnPropertyChanged();
            }
        }

        private string _propertyPath;

        public string PropertyPath
        {
            get { return _propertyPath; }
            set
            {
                _propertyPath = File.Exists(value) ? value : string.Empty;
                OnPropertyChanged();
            }
        }

        private bool _itemStartup;
        public bool ItemStartup
        {
            get { return _itemStartup; }
            set
            {
                _itemStartup = value;
                OnPropertyChanged();
            }
        }

        public static Wallpaper CreateDefaultWallpaper()
        {
            var defaultImgBuffers = Properties.Resources._default;

            var defaultWallpaperPath = Path.Combine(Program.WallpaperDir, "latecat.default.wallpaper");
            var defaultWallpaperFilepath = Path.Combine(Program.WallpaperDir, "latecat.default.wallpaper", "_default.jpg");

            if (!Directory.Exists(defaultWallpaperPath))
            {
                Directory.CreateDirectory(defaultWallpaperPath);
            }

            if (!File.Exists(defaultWallpaperFilepath))
            {
                using var file = File.Create(defaultWallpaperFilepath);

                file.Write(defaultImgBuffers);

                file.Flush();
                file.Close();
            }

            var wallpaperInfo = new WallpaperInfo()
            {
                AppVersion = "1.0.0.0",
                IsAbsolutePath = true,
                FileName = defaultWallpaperFilepath,
                Type = PoseidonEngine.Core.WallpaperType.Picture
            };

            Json<WallpaperInfo>.StoreData(Path.Combine(defaultWallpaperPath, "WallpaperInfo.json"), wallpaperInfo);

            return new(wallpaperInfo, defaultWallpaperPath, WallpaperProcessStatus.Ready);
        }

        public Wallpaper(WallpaperInfo wallpaperInfo, string folderPath, WallpaperProcessStatus status = WallpaperProcessStatus.Ready)
        {
            Status = status;
            WallpaperType = FileFilter.GetLocalisedWallpaperTypeString(wallpaperInfo.Type);
            WallpaperInfo = new WallpaperInfo(wallpaperInfo);
            Title = wallpaperInfo.Title;
            Desc = wallpaperInfo.Desc;
            Author = wallpaperInfo.Author;
            try
            {
                SrcWebsite = LinkHandler.SanitizeUrl(wallpaperInfo.Contact);
            }
            catch
            {
                SrcWebsite = null;
            }

            if (wallpaperInfo.IsAbsolutePath)
            {
                FilePath = wallpaperInfo.FileName;

                try
                {
                    PropertyPath = Path.Combine(Directory.GetParent(wallpaperInfo.FileName).ToString(), "Properties.json");
                }
                catch
                {
                    PropertyPath = null;
                }
            }
            else
            {
                if (wallpaperInfo.Type == PoseidonEngine.Core.WallpaperType.Url
                                || wallpaperInfo.Type == PoseidonEngine.Core.WallpaperType.VideoStream)
                {
                    FilePath = wallpaperInfo.FileName;
                }
                else
                {
                    try
                    {
                        FilePath = Path.Combine(folderPath, wallpaperInfo.FileName);
                    }
                    catch
                    {
                        FilePath = string.Empty;
                    }

                    try
                    {
                        PropertyPath = Path.Combine(folderPath, "Properties.json");
                    }
                    catch
                    {
                        PropertyPath = string.Empty;
                    }
                }
            }

            InfoFolderPath = folderPath;

            if (wallpaperInfo.Type == PoseidonEngine.Core.WallpaperType.Video ||
                wallpaperInfo.Type == PoseidonEngine.Core.WallpaperType.VideoStream ||
                wallpaperInfo.Type == PoseidonEngine.Core.WallpaperType.Gif ||
                wallpaperInfo.Type == PoseidonEngine.Core.WallpaperType.Picture)
            {
                if (PropertyPath == null)
                {
                    PropertyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                        "plugins", "mpv", "api", "Properties.json");
                }
            }

            ItemStartup = false;
        }
    }
}