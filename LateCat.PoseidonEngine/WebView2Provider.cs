using LateCat.PoseidonEngine.Abstractions;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System.Threading.Tasks;
using System.Windows;

namespace LateCat.PoseidonEngine.Core
{
    public class WebView2Provider : IWebView2Provider
    {
        private readonly WebView2 _webView2;
        private CoreWebView2Environment _env;

        public WebView2Provider()
        {
            _webView2 = new();
        }

        public WebView2 GetWebView2()
            => _webView2;

        public async Task EnsureCoreWebView2Async()
        {
            try
            {
                if (_env is null)
                {
                    _env = await CoreWebView2Environment.CreateAsync(null, Constants.Paths.TempWebView2Dir).ConfigureAwait(false);
                }

                _webView2.CoreWebView2InitializationCompleted += WebView2_CoreWebView2InitializationCompleted;

                await _webView2.EnsureCoreWebView2Async(_env).ConfigureAwait(false);
            }
            catch
            {

            }
        }

        private void WebView2_CoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            _webView2.HorizontalAlignment = HorizontalAlignment.Center;
            _webView2.VerticalAlignment = VerticalAlignment.Center;

            _webView2.CoreWebView2.Settings.AreDevToolsEnabled = false;
            _webView2.CoreWebView2.Settings.IsStatusBarEnabled = false;
            _webView2.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
            _webView2.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;

            _webView2.CoreWebView2.Navigate("about:blank");
        }
    }
}