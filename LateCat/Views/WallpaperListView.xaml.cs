using LateCat.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using System.Windows.Controls;

namespace LateCat.Views
{
    public partial class WallpaperListView : Page
    {
        public event EventHandler<object> ContextMenuClick;
        public event EventHandler<DragEventArgs> FileDroppedEvent;
        private readonly MainWindow _mainWnd;

        public WallpaperListView()
        {
            InitializeComponent();

            DataContext = App.Services.GetRequiredService<WallpaperListViewModel>();

            _mainWnd = App.Services.GetRequiredService<MainWindow>();
            _mainWnd.Loading.Visibility = Visibility.Visible;

            Media.SourceUpdated += Media_SourceUpdated;
            Media.MediaEnded += Media_MediaEnded;
            Media.MediaOpened += Media_MediaOpened;
        }

        private void Media_MediaOpened(object sender, RoutedEventArgs e)
        {
            _mainWnd.Loading.Visibility = Visibility.Collapsed;
        }

        private void Media_MediaEnded(object sender, RoutedEventArgs e)
        {
            Media.LoadedBehavior = MediaState.Manual;
            ((MediaElement)sender).Stop();
            ((MediaElement)sender).Play();
        }

        private void Media_SourceUpdated(object? sender, System.Windows.Data.DataTransferEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Page_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }

            Mask.Visibility = Visibility.Visible;
        }

        private void Page_DragLeave(object sender, DragEventArgs e)
        {
            Mask.Visibility = Visibility.Collapsed;
        }

        private void Page_Drop(object sender, DragEventArgs e)
        {
            FileDroppedEvent?.Invoke(sender, e);

            Mask.Visibility = Visibility.Collapsed;
        }
    }
}
