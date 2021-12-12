using LateCat.Common;
using LateCat.Helpers;
using LateCat.Models;
using LateCat.PoseidonEngine.Abstractions;
using LateCat.PoseidonEngine.Core;
using LateCat.PoseidonEngine.Models;
using LateCat.PoseidonEngine.Utilities;
using LateCat.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;

namespace LateCat.ViewModels
{
    class WallpaperPreviewViewModel : ObservableObject
    {
        private readonly IWallpaperMetadata _metadata;
        private readonly IWallpaperPreview Winstance;
        private readonly IWallpaperInfo _infoCopy;

        private readonly ISettingsService _settings;
        private readonly WallpaperListViewModel _libraryVm;
        private bool _ok = false;
        public bool Ok { get => _ok; internal set => _ok = value; }

        public WallpaperPreviewViewModel(IWallpaperPreview wInterface, IWallpaper wallpaper)
        {
            _settings = App.Services.GetRequiredService<ISettingsService>();
            _libraryVm = App.Services.GetRequiredService<WallpaperListViewModel>();

            Winstance = wInterface;

            _metadata = wallpaper.Metadata;
            if (_metadata.Status == WallpaperProcessStatus.Edit)
            {
                _infoCopy = new WallpaperInfo(_metadata.WallpaperInfo);

                Title = _metadata.WallpaperInfo.Title;
                Desc = _metadata.WallpaperInfo.Desc;
                Url = _metadata.WallpaperInfo.Contact;
                Author = _metadata.WallpaperInfo.Author;
            }
            else
            {
                if (_metadata.WallpaperInfo.Type == WallpaperType.VideoStream)
                {
                    Url = _metadata.FilePath;
                    Title = GetLastSegmentUrl(_metadata.FilePath);
                }
                else if (_metadata.WallpaperInfo.Type == WallpaperType.Url
                    || _metadata.WallpaperInfo.Type == WallpaperType.Web
                    || _metadata.WallpaperInfo.Type == WallpaperType.WebAudio)
                {
                    if (_metadata.WallpaperInfo.Type == WallpaperType.Url)
                        Url = _metadata.FilePath;

                    Title = GetLastSegmentUrl(_metadata.FilePath);
                }
                else
                {
                    try
                    {
                        Title = Path.GetFileNameWithoutExtension(_metadata.FilePath);
                    }
                    catch (ArgumentException)
                    {
                        Title = _metadata.FilePath;
                    }

                    if (string.IsNullOrWhiteSpace(Title))
                    {
                        Title = _metadata.FilePath;
                    }
                }

                if (_metadata.Status == WallpaperProcessStatus.CmdImport ||
                    _metadata.Status == WallpaperProcessStatus.MultiImport)
                {
                    wallpaper.SetPlaybackPos(35, PlaybackPosType.Absolute);
                }
            }
        }

        #region data

        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value?.Length > 100 ? value[..100] : value;
                _metadata.Title = _title;
                _metadata.WallpaperInfo.Title = _title;
                OnPropertyChanged();
            }
        }

        private string _desc;
        public string Desc
        {
            get { return _desc; }
            set
            {
                _desc = value?.Length > 5000 ? value[..5000] : value;
                _metadata.Desc = _desc;
                _metadata.WallpaperInfo.Desc = _desc;
                OnPropertyChanged();
            }
        }

        private string _author;
        public string Author
        {
            get { return _author; }
            set
            {
                _author = value?.Length > 100 ? value[..100] : value;
                _metadata.Author = _author;
                _metadata.WallpaperInfo.Author = _author;
                OnPropertyChanged();
            }
        }

        private string _url;
        public string Url
        {
            get { return _url; }
            set
            {
                _url = value;
                try
                {
                    _metadata.SrcWebsite = LinkHandler.SanitizeUrl(_url);
                }
                catch
                {
                    _metadata.SrcWebsite = null;
                }
                _metadata.WallpaperInfo.Contact = _url;
                OnPropertyChanged();
            }
        }

        #endregion data

        #region ui 

        public void OnWindowClosed(object? sender, EventArgs e)
        {
            CleanUp();
        }

        #endregion ui

        #region interface methods


        private RelayCommand _cancelCommand;
        public RelayCommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new RelayCommand(
                        param => Winstance.Exit());
                }
                return _cancelCommand;
            }
            private set
            {
                _cancelCommand = value;
            }
        }

        private RelayCommand _okCommand;
        public RelayCommand OkCommand
        {
            get
            {
                if (_okCommand is null)
                {
                    _okCommand = new RelayCommand(
                        param => Winstance.Ok());
                }

                return _okCommand;
            }
            private set
            {
                _okCommand = value;
            }
        }

        private void CleanUp()
        {
            if (_metadata.Status == WallpaperProcessStatus.Edit)
            {
                Title = _infoCopy.Title;
                Desc = _infoCopy.Desc;
                Author = _infoCopy.Author;
                Url = _infoCopy.Contact;

                _metadata.Status = WallpaperProcessStatus.Ready;
                _libraryVm.SortLibraryItem((Wallpaper)_metadata);
            }
            else
            {
                if (_ok)
                {
                    Json<WallpaperInfo>.StoreData(
                        Path.Combine(_metadata.InfoFolderPath, "WallpaperInfo.json"), _metadata.WallpaperInfo);

                    _metadata.Status = WallpaperProcessStatus.Ready;
                    _libraryVm.SortLibraryItem((Wallpaper)_metadata);
                }
            }
        }

        #endregion interface methods

        #region helpers

        private string GetLastSegmentUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                var segment = uri.Segments.Last();
                return (segment == "/" || segment == "//") ? uri.Host.Replace("www.", string.Empty) : segment.Replace("/", string.Empty);
            }
            catch
            {
                return url;
            }
        }

        #endregion helpers
    }
}
