using LateCat.PoseidonEngine.Abstractions;
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

        private readonly WallpaperPreviewerViewModel _previewerVm;

        public WallpaperListView()
        {
            InitializeComponent();

            DataContext = App.Services.GetRequiredService<WallpaperListViewModel>();

            _mainWnd = App.Services.GetRequiredService<MainWindow>();
            _mainWnd.Loading.Visibility = Visibility.Visible;

            _previewerVm = App.Services.GetRequiredService<WallpaperPreviewerViewModel>();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Program.PreviewerWidth = ActualWidth;
            Program.PreviewerHeight = ActualHeight;            
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

        private void Previewer_SourceChanged(object sender, RoutedEventArgs e)
        {
            (Previewer.Content as IPreviewer)?.Close();

            _previewerVm.OnWallpaperPreviewerSourceChanged(Previewer.Source);

            Previewer.Content = _previewerVm.GetWallpaperPreviewer();

            (Previewer.Content as IPreviewer).Preview();
        }
    }
}
