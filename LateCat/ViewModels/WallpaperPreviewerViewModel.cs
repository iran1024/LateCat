using LateCat.Common;
using LateCat.PoseidonEngine.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace LateCat.ViewModels
{
    internal class WallpaperPreviewerViewModel : ObservableObject
    {
        private readonly IPreviewerProvider _previewerProvider;

        private IWallpaperMetadata _wallpaper;
        public IWallpaperMetadata Wallpaper
        {
            get => _wallpaper;
            set
            {
                _wallpaper = value;
                OnPropertyChanged();
            }
        }

        public WallpaperPreviewerViewModel()
        {
            _previewerProvider = App.Services.GetRequiredService<IPreviewerProvider>();
        }

        internal IPreviewer GetWallpaperPreviewer()
        {
            return _previewerProvider.GetPreviewer(_wallpaper);
        }

        public void OnWallpaperPreviewerSourceChanged(IWallpaperMetadata metadata)
        {
            Wallpaper = metadata;
        }
    }
}
