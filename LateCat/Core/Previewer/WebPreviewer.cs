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
        private WebView2Element _wv2Wnd;

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
            _wv2Wnd = new WebView2Element(metadata.FilePath, metadata.WallpaperInfo.Type, metadata.PropertyPath);
            _wv2Wnd.WebView2.NavigationCompleted += WebView_NavigationCompleted;

            return this;
        }

        private void WebView_NavigationCompleted(
            object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            if (_wv2Wnd.WebView2.CoreWebView2.Source.Equals("about:blank"))
            {
                return;
            }

            WindowOperations.SetProgramToFramework(Window.GetWindow(this), new WindowInteropHelper(_wv2Wnd).Handle, this);

            App.Services.GetRequiredService<MainWindow>().Loading.Visibility = Visibility.Collapsed;
        }

        public async void Preview()
        {
            if (_wv2Wnd is null)
            {
                ChangeSource(Metadata);
            }

            _wv2Wnd.Show();

            Width = Program.PreviewerWidth;
            Height = Program.PreviewerHeight;

            try
            {
                await _wv2Wnd.InitializeWebView2();
            }
            catch
            { }
        }

        public void Close()
        {
            Height = 0;
            Width = 0;
            RenderSize = new Size(0, 0);

            if (_wv2Wnd is not null)
            {
                _wv2Wnd.WebView2.NavigationCompleted -= WebView_NavigationCompleted;

                _wv2Wnd.Close();

                _wv2Wnd = null;
            }            
        }
    }
}
