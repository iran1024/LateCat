using LateCat.PoseidonEngine.Abstractions;
using LateCat.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace LateCat.Core
{
    internal class WebPreviewer : Border, IPreviewer
    {
        private bool _isInit = false;
        private WebView2Element _webView2;

        public WebPreviewer()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;
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
            _webView2 = new WebView2Element(metadata.FilePath, metadata.WallpaperInfo.Type, metadata.PropertyPath);

            return this;
        }

        public async void Preview()
        {
            App.Services.GetRequiredService<MainWindow>().Loading.Visibility = Visibility.Visible;

            if (_webView2 != null)
            {
                Width = Program.PreviewerWidth;
                Height = Program.PreviewerHeight;

                _webView2.Show();

                var hWnd = new WindowInteropHelper(_webView2).Handle;

                try
                {
                    await _webView2.InitializeWebView();
                }
                catch
                {

                }
                finally
                {
                    WindowOperations.SetProgramToFramework(Window.GetWindow(this), hWnd, this);

                    App.Services.GetRequiredService<MainWindow>().Loading.Visibility = Visibility.Collapsed;
                }
            }
        }

        public void Close()
        {
            _webView2.Close();
        }
    }
}
