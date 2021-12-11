using LateCat.Core.Cef;
using LateCat.PoseidonEngine.Abstractions;
using LateCat.PoseidonEngine.Core;
using LateCat.PoseidonEngine.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace LateCat.Views
{
    public partial class WebView2Element : Window
    {
        private readonly WebView2 _wv2;

        private readonly string _htmlPath;
        private readonly string _propertyPath;
        private readonly WallpaperType _wallpaperType;

        public event EventHandler PropertiesInitialized;

        public WebView2 WebView2 => _wv2;

        public WebView2Element(string path, WallpaperType type, string propertyPath)
        {
            InitializeComponent();

            Loaded += WebView2Element_Loaded;

            _htmlPath = path;
            _propertyPath = propertyPath;
            _wallpaperType = type;

            Content = _wv2 = App.Services.GetRequiredService<IWebView2Provider>().GetWebView2();
        }

        public async Task<IntPtr> InitializeWebView2()
        {
            await App.Services.GetRequiredService<IWebView2Provider>().EnsureCoreWebView2Async();

            if (_wallpaperType == WallpaperType.Url)
            {
                string tmp = string.Empty;
                if (TryParseShadertoy(_htmlPath, ref tmp))
                {
                    _wv2.CoreWebView2.NavigateToString(tmp);
                }
                else if ((tmp = StreamHelper.GetYouTubeVideoIdFromUrl(_htmlPath)) != "")
                {
                    _wv2.CoreWebView2.Navigate("https://www.youtube.com/embed/" + tmp +
                        "?version=3&rel=0&autoplay=1&loop=1&controls=0&playlist=" + tmp);
                }
                else
                {
                    _wv2.CoreWebView2.Navigate(_htmlPath);
                }
            }
            else
            {
                _wv2.CoreWebView2.Navigate(_htmlPath);
            }

            await ExecuteScrpitAsync(@"document.body.parentNode.style.overflowY = 'hidden';");
            await ExecuteScrpitAsync(@"document.ondragover = function (e) { e.preventDefault(); return false; }");
            await ExecuteScrpitAsync(@"document.ondrop = function (e) { e.preventDefault(); return false; }");

            return _wv2.Handle;
        }

        private async Task<string> ExecuteScrpitAsync(string javascript)
        {
            await _wv2.CoreWebView2.ExecuteScriptAsync(javascript);
            return await _wv2.ExecuteScriptAsync(javascript);
        }

        private async void WebView2Element_Loaded(object sender, RoutedEventArgs e)
        {
            WindowOperator.RemoveWindowFromTaskbar(new WindowInteropHelper(this).Handle);

            ShowInTaskbar = false;
            ShowInTaskbar = true;

            await App.Services.GetRequiredService<IWebView2Provider>().EnsureCoreWebView2Async();

            _wv2.CoreWebView2.ProcessFailed += CoreWebView2_ProcessFailed;

            _wv2.Width = Program.PreviewerWidth;
            _wv2.Height = Program.PreviewerHeight;
        }

        private async void CoreWebView2_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            await RestoreProperties(_propertyPath);

            PropertiesInitialized?.Invoke(this, EventArgs.Empty);
        }

        public void MessageProcess(IPCMessage obj)
        {
            try
            {
                switch (obj.Type)
                {
                    case MessageType.cmd_reload:
                        _wv2?.Reload();
                        break;
                    case MessageType.lp_slider:
                        var sl = (IPCSlider)obj;
                        _ = ExecuteScriptFunctionAsync("LateCatPropertyListener", sl.Name, sl.Value);
                        break;
                    case MessageType.lp_textbox:
                        var tb = (IPCTextBox)obj;
                        _ = ExecuteScriptFunctionAsync("LateCatPropertyListener", tb.Name, tb.Value);
                        break;
                    case MessageType.lp_dropdown:
                        var dd = (IPCDropdown)obj;
                        _ = ExecuteScriptFunctionAsync("LateCatPropertyListener", dd.Name, dd.Value);
                        break;
                    case MessageType.lp_cpicker:
                        var cp = (IPCColorPicker)obj;
                        _ = ExecuteScriptFunctionAsync("LateCatPropertyListener", cp.Name, cp.Value);
                        break;
                    case MessageType.lp_chekbox:
                        var cb = (IPCCheckbox)obj;
                        _ = ExecuteScriptFunctionAsync("LateCatPropertyListener", cb.Name, cb.Value);
                        break;
                    case MessageType.lp_fdropdown:
                        var fd = (IPCFolderDropdown)obj;
                        var filePath = Path.Combine(Path.GetDirectoryName(_htmlPath)!, fd.Value);
                        if (File.Exists(filePath))
                        {
                            _ = ExecuteScriptFunctionAsync("LateCatPropertyListener",
                            fd.Name,
                            fd.Value);
                        }
                        else
                        {
                            _ = ExecuteScriptFunctionAsync("LateCatPropertyListener",
                            fd.Name,
                            null);
                        }
                        break;
                    case MessageType.lp_button:
                        var btn = (IPCButton)obj;
                        if (btn.IsDefault)
                        {
                            _ = RestoreProperties(_propertyPath);
                        }
                        else
                        {
                            _ = ExecuteScriptFunctionAsync("LateCatPropertyListener", btn.Name, true);
                        }
                        break;
                    case MessageType.lsp_perfcntr:
                        break;
                }
            }
            catch
            {

            }
        }

        public void MessageProcess(string msg) =>
            MessageProcess(JsonConvert.DeserializeObject<IPCMessage>(msg, new JsonSerializerSettings() { Converters = { new IPCMessageConverter() } }));

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                _wv2.CoreWebView2.Navigate("about:blank");
                _wv2.CoreWebView2.ProcessFailed -= CoreWebView2_ProcessFailed;
            }
            catch
            {

            }
        }

        private async void CoreWebView2_ProcessFailed(object? sender, CoreWebView2ProcessFailedEventArgs e)
        {
            await App.Services.GetRequiredService<IWebView2Provider>().EnsureCoreWebView2Async();
        }

        #region helpers

        private async Task<string> ExecuteScriptFunctionAsync(string functionName, params object[] parameters)
        {
            var script = new StringBuilder();
            script.Append(functionName);
            script.Append('(');

            for (int i = 0; i < parameters.Length; i++)
            {
                script.Append(JsonConvert.SerializeObject(parameters[i]));
                if (i < parameters.Length - 1)
                {
                    script.Append(", ");
                }
            }
            script.Append(");");

            return await _wv2.ExecuteScriptAsync(script.ToString());
        }

        private async Task RestoreProperties(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                    return;

                foreach (var item in PropertiesJsonHelper.LoadProperties(path))
                {
                    string uiElementType = item.Value["type"].ToString();
                    if (!uiElementType.Equals("button", StringComparison.OrdinalIgnoreCase) && !uiElementType.Equals("label", StringComparison.OrdinalIgnoreCase))
                    {
                        if (uiElementType.Equals("slider", StringComparison.OrdinalIgnoreCase) ||
                            uiElementType.Equals("dropdown", StringComparison.OrdinalIgnoreCase))
                        {
                            await ExecuteScriptFunctionAsync("LateCatPropertyListener", item.Key, (int)item.Value["value"]);
                        }
                        else if (uiElementType.Equals("folderDropdown", StringComparison.OrdinalIgnoreCase))
                        {
                            var filePath = Path.Combine(Path.GetDirectoryName(_htmlPath), item.Value["folder"].ToString(), item.Value["value"].ToString());
                            if (File.Exists(filePath))
                            {
                                await ExecuteScriptFunctionAsync("LateCatPropertyListener",
                                item.Key,
                                Path.Combine(item.Value["folder"].ToString(), item.Value["value"].ToString()));
                            }
                            else
                            {
                                await ExecuteScriptFunctionAsync("LateCatPropertyListener",
                                item.Key,
                                null);
                            }
                        }
                        else if (uiElementType.Equals("checkbox", StringComparison.OrdinalIgnoreCase))
                        {
                            await ExecuteScriptFunctionAsync("LateCatPropertyListener", item.Key, (bool)item.Value["value"]);
                        }
                        else if (uiElementType.Equals("color", StringComparison.OrdinalIgnoreCase) || uiElementType.Equals("textbox", StringComparison.OrdinalIgnoreCase))
                        {
                            await ExecuteScriptFunctionAsync("LateCatPropertyListener", item.Key, (string)item.Value["value"]);
                        }
                    }
                }
            }
            catch
            {

            }
        }


        private static bool TryParseShadertoy(string url, ref string html)
        {
            if (!url.Contains("shadertoy.com/view"))
            {
                return false;
            }

            try
            {
                _ = LinkHandler.SanitizeUrl(url);
            }
            catch
            {
                return false;
            }

            url = url.Replace("view/", "embed/");
            html = @"<!DOCTYPE html><html lang=""en"" dir=""ltr""> <head> <meta charset=""utf - 8""> 
                    <title>Digital Brain</title> <style media=""Monitor""> iframe { position: fixed; width: 100%; height: 100%; top: 0; right: 0; bottom: 0;
                    left: 0; z-index; -1; pointer-events: none;  } </style> </head> <body> <iframe width=""640"" height=""360"" frameborder=""0"" 
                    src=" + url + @"?gui=false&t=10&paused=false&muted=true""></iframe> </body></html>";
            return true;
        }

        //ref: https://github.com/MicrosoftEdge/WebView2Feedback/issues/529
        public async Task CaptureScreenshot(string filePath, ScreenshotFormat format)
        {
            var base64String = await CaptureScreenshot(format);
            var imageBytes = Convert.FromBase64String(base64String);
            switch (format)
            {
                case ScreenshotFormat.jpeg:
                case ScreenshotFormat.png:
                case ScreenshotFormat.webp:
                    {
                        // Write to disk
                        File.WriteAllBytes(filePath, imageBytes);
                    }
                    break;
                case ScreenshotFormat.bmp:
                    {
                        // Convert byte[] to Image
                        using MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
                        using Image image = Image.FromStream(ms, true);
                        image.Save(filePath, ImageFormat.Bmp);
                    }
                    break;
            }
        }

        private async Task<string> CaptureScreenshot(ScreenshotFormat format)
        {
            var param = format switch
            {
                ScreenshotFormat.jpeg => "{\"format\":\"jpeg\"}",
                ScreenshotFormat.webp => "{\"format\":\"webp\"}",
                ScreenshotFormat.png => "{}", // Default
                ScreenshotFormat.bmp => "{}", // Not supported by cef
                _ => "{}",
            };
            string r3 = await _wv2.CoreWebView2.CallDevToolsProtocolMethodAsync("Page.captureMonitorshot", param);
            JObject o3 = JObject.Parse(r3);
            JToken data = o3["data"];
            return data.ToString();
        }

        public static Image Base64ToImage(string base64String)
        {
            // Convert base 64 string to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            // Convert byte[] to Image
            using MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
            Image image = Image.FromStream(ms, true);
            return image;
        }

        public static BitmapImage Base64ToBitmapImage(string base64String)
        {
            var imageBytes = Convert.FromBase64String(base64String);
            var bi = new BitmapImage();
            using var ms = new MemoryStream(imageBytes);
            bi.BeginInit();
            bi.CacheOption = BitmapCacheOption.OnLoad;
            //bi.DecodePixelWidth = 1280;
            bi.StreamSource = ms;
            bi.EndInit();
            return bi;
        }

        #endregion //helpers
    }
}