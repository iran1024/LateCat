using LateCat.PoseidonEngine.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LateCat.Core
{
    internal class VideoPreviewer : MediaElement, IPreviewer
    {
        public VideoPreviewer()
        {
            LoadedBehavior = MediaState.Manual;
            Stretch = Stretch.UniformToFill;
            HorizontalAlignment = HorizontalAlignment.Center;
            VerticalAlignment = VerticalAlignment.Center;

            MediaOpened += VideoPreviewer_MediaOpened;
            MediaEnded += VideoPreviewer_MediaEnded;
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
            Source = new Uri(metadata.FilePath);

            return this;
        }

        public void Preview()
        {
            Play();
        }

        public new void Close()
        {
            base.Close();
        }

        private void VideoPreviewer_MediaOpened(object sender, RoutedEventArgs e)
        {
            App.Services.GetRequiredService<MainWindow>().Loading.Visibility = Visibility.Collapsed;
        }

        private void VideoPreviewer_MediaEnded(object sender, RoutedEventArgs e)
        {
            Position = TimeSpan.Zero;
            Play();
        }
    }
}
