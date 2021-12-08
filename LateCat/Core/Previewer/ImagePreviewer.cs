using LateCat.PoseidonEngine.Abstractions;
using LateCat.PoseidonEngine.Core;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LateCat.Core
{
    internal class ImagePreviewer : Image, IPreviewer
    {
        public ImagePreviewer()
        {
            Stretch = Stretch.UniformToFill;
            HorizontalAlignment = HorizontalAlignment.Center;
            VerticalAlignment = VerticalAlignment.Center;
        }

        public IWallpaperMetadata Metadata { get; private set; }

        public IPreviewer GetPreviewer(IWallpaperMetadata metadata)
        {
            if (Metadata is null || !Metadata.Equals(metadata))
            {
                Metadata = metadata;
            }

            return ChangeSource(metadata);
        }

        public IPreviewer ChangeSource(IWallpaperMetadata metadata)
        {
            var bi = new BitmapImage(new Uri(metadata.FilePath))
            {
                CreateOptions = BitmapCreateOptions.DelayCreation,
                CacheOption = BitmapCacheOption.OnLoad,
            };

            Source = bi;

            return this;
        }

        public void Preview()
        {

        }

        public void Close()
        {
            
        }

        protected override void OnRender(DrawingContext dc)
        {
            App.Services.GetRequiredService<MainWindow>().Loading.Visibility = Visibility.Collapsed;

            base.OnRender(dc);
        }
    }
}