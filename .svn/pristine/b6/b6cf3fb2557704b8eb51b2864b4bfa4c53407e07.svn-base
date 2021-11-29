using CefSharp;
using CefSharp.SchemeHandler;
using CefSharp.WinForms;
using CommandLine;
using LateCat.Cefplayer;
using LateCat.CefPlayer.Helpers;
using LateCat.CefPlayer.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace LateCat.CefPlayer
{
    public partial class DefaultForm : Form
    {
        private enum LinkType
        {
            shadertoy,
            yt,
            online,
            local,
            standalone
        }

        private LinkType linkType;
        private string originalUrl;
        private string debugPort;
        private string cachePath;
        private int cefVolume;
        private bool sysAudioEnabled;
        private bool sysMonitorEnabled;
        public string htmlPath;
        private string propertyPath;
        private string path;
        private Rectangle preferredWinSize = Rectangle.Empty;
        private JObject propertyData;
        private bool VerboseLog;
        private bool suspendJsMsg = false;
        private readonly PerfCounterUsage sysMonitor;
        private readonly SystemAudio sysAudio;
        private ChromiumWebBrowser chromeBrowser;

        public DefaultForm()
        {
            InitializeComponent();            

            WindowState = FormWindowState.Normal;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(-9999, 0);

            Parser.Default.ParseArguments<StartArgs>(Environment.GetCommandLineArgs())
                .WithParsed(RunOptions)
                .WithNotParsed(HandleParseError);

            var sb = new StringBuilder();
            foreach (var item in Environment.GetCommandLineArgs())
            {
                sb.Append(item);
            }

            MessageBox.Show($"参数：{sb}");

            if (preferredWinSize != Rectangle.Empty)
            {
                Size = new Size(preferredWinSize.Width, preferredWinSize.Height);
            }

            StdInListener();

            try
            {
                propertyData = JsonUtil.Read(propertyPath);
            }
            catch
            {

            }

            StartCef();

            sysMonitorEnabled = linkType == LinkType.local && sysMonitorEnabled;
            sysAudioEnabled = linkType == LinkType.local && sysAudioEnabled;

            if (sysAudioEnabled)
            {
                sysAudio = new SystemAudio();
                sysAudio.AudioData += SysAudio_AudioData;
                sysAudio.Start();
            }

            if (sysMonitorEnabled)
            {
                sysMonitor = new PerfCounterUsage();
                sysMonitor.HWMonitor += SysMonitor_HardwareUsage;
                sysMonitor.Start();
            }
        }

        private void RunOptions(StartArgs opts)
        {
            path = opts.Url;
            htmlPath = path;
            originalUrl = opts.Url;
            sysAudioEnabled = opts.AudioAnalyse;
            sysMonitorEnabled = opts.SysInfo;
            debugPort = opts.DebugPort;
            cachePath = opts.CachePath;
            cefVolume = opts.Volume;
            VerboseLog = opts.VerboseLog;

            if (opts.Geometry != null)
            {
                var msg = opts.Geometry.Split('x');
                if (msg.Length >= 2 && int.TryParse(msg[0], out int width) && int.TryParse(msg[1], out int height))
                {
                    preferredWinSize = new Rectangle(0, 0, width, height);
                }
            }

            if (opts.Type.Equals("local", StringComparison.OrdinalIgnoreCase))
            {
                linkType = LinkType.local;
            }
            else if (opts.Type.Equals("online", StringComparison.OrdinalIgnoreCase))
            {
                string tmp = null;
                if (LinkUtil.TryParseShadertoy(path, ref tmp))
                {
                    linkType = LinkType.shadertoy;
                    path = tmp;
                }
                else if ((tmp = LinkUtil.GetYouTubeVideoIdFromUrl(htmlPath)) != "")
                {
                    linkType = LinkType.yt;
                    path = "https://www.youtube.com/embed/" + tmp +
                        "?version=3&rel=0&autoplay=1&loop=1&controls=0&playlist=" + tmp;
                }
                else
                {
                    linkType = LinkType.online;
                }
            }
            else if (opts.Type.Equals("deviantart", StringComparison.OrdinalIgnoreCase))
            {
                linkType = LinkType.standalone;

                FormBorderStyle = FormBorderStyle.Sizable;
                WindowState = FormWindowState.Maximized;
                ShowInTaskbar = true;
                MaximizeBox = true;
                MinimizeBox = true;
            }
            propertyPath = opts.Properties;
        }

        private void HandleParseError(IEnumerable<Error> errs)
        {
            if (Application.MessageLoop)
                Application.Exit();
            else
                Environment.Exit(1);
        }

        #region ipc

        public async void StdInListener()
        {
            try
            {
                await Task.Run(async () =>
                {
                    while (true)
                    {
                        string text = await Console.In.ReadLineAsync();
                        if (VerboseLog)
                        {
                            Console.WriteLine(text);
                        }

                        if (string.IsNullOrEmpty(text))
                        {

                            break;
                        }
                        else
                        {
                            try
                            {
                                var close = false;
                                var obj = JsonConvert.DeserializeObject<IPCMessage>(text, new JsonSerializerSettings() { Converters = { new IPCMessageConverter() } });
                                switch (obj.Type)
                                {
                                    case MessageType.cmd_reload:
                                        chromeBrowser?.Reload(true);
                                        break;
                                    case MessageType.cmd_suspend:
                                        suspendJsMsg = true;
                                        break;
                                    case MessageType.cmd_resume:
                                        suspendJsMsg = false;
                                        break;
                                    case MessageType.cmd_screenshot:
                                        var success = true;
                                        var scr = (IPCMonitorshotCmd)obj;
                                        try
                                        {
                                            await CaptureScreenshot(chromeBrowser, scr.FilePath, scr.Format);
                                        }
                                        catch (Exception ie)
                                        {
                                            success = false;
                                            WriteToParent(new IPCMessageConsole()
                                            {
                                                Category = ConsoleMessageType.error,
                                                Message = $"Screenshot capture fail: {ie.Message}"
                                            });
                                        }
                                        finally
                                        {
                                            WriteToParent(new IPCMessageMonitorshot()
                                            {
                                                FileName = Path.GetFileName(scr.FilePath),
                                                Success = success
                                            });
                                        }
                                        break;
                                    case MessageType.lp_slider:
                                        var sl = (IPCSlider)obj;
                                        chromeBrowser.ExecuteScriptAsync("LateCatPropertyListener", sl.Name, sl.Value);
                                        break;
                                    case MessageType.lp_textbox:
                                        var tb = (IPCTextBox)obj;
                                        chromeBrowser.ExecuteScriptAsync("LateCatPropertyListener", tb.Name, tb.Value);
                                        break;
                                    case MessageType.lp_dropdown:
                                        var dd = (IPCDropdown)obj;
                                        chromeBrowser.ExecuteScriptAsync("LateCatPropertyListener", dd.Name, dd.Value);
                                        break;
                                    case MessageType.lp_cpicker:
                                        var cp = (IPCColorPicker)obj;
                                        chromeBrowser.ExecuteScriptAsync("LateCatPropertyListener", cp.Name, cp.Value);
                                        break;
                                    case MessageType.lp_chekbox:
                                        var cb = (IPCCheckbox)obj;
                                        chromeBrowser.ExecuteScriptAsync("LateCatPropertyListener", cb.Name, cb.Value);
                                        break;
                                    case MessageType.lp_fdropdown:
                                        var fd = (IPCFolderDropdown)obj;
                                        var filePath = Path.Combine(Path.GetDirectoryName(htmlPath), fd.Value);
                                        if (File.Exists(filePath))
                                        {
                                            chromeBrowser.ExecuteScriptAsync("LateCatPropertyListener",
                                            fd.Name,
                                            fd.Value);
                                        }
                                        else
                                        {
                                            chromeBrowser.ExecuteScriptAsync("LateCatPropertyListener",
                                            fd.Name,
                                            null); //or custom msg
                                        }
                                        break;
                                    case MessageType.lp_button:
                                        var btn = (IPCButton)obj;
                                        if (btn.IsDefault)
                                        {
                                            try
                                            {
                                                //load new file.
                                                propertyData = JsonUtil.Read(propertyPath);
                                                //restore new property values.
                                                RestoreLivelyPropertySettings();
                                            }
                                            catch (Exception ex)
                                            {
                                                MessageBox.Show(ex.ToString(), "Lively Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                            }
                                        }
                                        else
                                        {
                                            chromeBrowser.ExecuteScriptAsync("LateCatPropertyListener", btn.Name, true);
                                        }
                                        break;
                                    case MessageType.lsp_perfcntr:
                                        if (chromeBrowser.CanExecuteJavascriptInMainFrame) //if js context ready
                                        {
                                            chromeBrowser.ExecuteScriptAsync("IPCSystemInformation", JsonConvert.SerializeObject(((IPCSystemInformation)obj).Info, Formatting.Indented));
                                        }
                                        break;
                                    case MessageType.lsp_nowplaying:
                                        if (chromeBrowser.CanExecuteJavascriptInMainFrame)
                                        {
                                            chromeBrowser.ExecuteScriptAsync("livelyCurrentTrack", JsonConvert.SerializeObject(((IPCSystemNowPlaying)obj).Info, Formatting.Indented));
                                        }
                                        break;
                                    case MessageType.cmd_close:
                                        close = true;
                                        break;
                                }

                                if (close)
                                {
                                    break;
                                }
                            }
                            catch (Exception e)
                            {
                                WriteToParent(new IPCMessageConsole()
                                {
                                    Category = ConsoleMessageType.error,
                                    Message = $"Ipc parse error: {e.Message}"
                                });
                            }
                        }
                    }
                });
            }
            catch (Exception e)
            {
                WriteToParent(new IPCMessageConsole()
                {
                    Category = ConsoleMessageType.error,
                    Message = $"Ipc stdin error: {e.Message}",
                });
            }
            finally
            {
                sysAudio?.Dispose();
                sysMonitor?.Stop();
                chromeBrowser?.Dispose();
                Cef.Shutdown();
                Application.Exit();
            }
        }

        public static void WriteToParent(IPCMessage obj)
        {
            Console.WriteLine(JsonConvert.SerializeObject(obj));
        }

        #endregion //ipc

        #region cef

        public void StartCef()
        {
            var settings = new CefSettings();

            if (cefVolume == 0)
            {
                settings.CefCommandLineArgs.Add("--mute-audio", "1");
            }

            settings.CefCommandLineArgs.Add("autoplay-policy", "no-user-gesture-required");
            settings.LogFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "LateCat", "cef", "logfile.txt");

            if (!string.IsNullOrWhiteSpace(debugPort))
            {
                if (int.TryParse(debugPort, out int value))
                    settings.RemoteDebuggingPort = value;
            }

            if (!string.IsNullOrWhiteSpace(cachePath))
            {
                settings.CachePath = cachePath;
            }
            else
            {
                settings.CefCommandLineArgs.Add("--disable-gpu-shader-disk-cache");
            }

            switch (linkType)
            {
                case LinkType.shadertoy:
                    {
                        Cef.Initialize(settings);
                        chromeBrowser = new ChromiumWebBrowser(string.Empty);
                        chromeBrowser.LoadHtml(path);
                    }
                    break;
                case LinkType.yt:
                    {
                        Cef.Initialize(settings);
                        chromeBrowser = new ChromiumWebBrowser(string.Empty);
                        chromeBrowser.Load(path);
                    }
                    break;
                case LinkType.local:
                    {
                        settings.RegisterScheme(new CefCustomScheme
                        {
                            SchemeName = "localfolder",
                            IsFetchEnabled = true,
                            SchemeHandlerFactory = new FolderSchemeHandlerFactory
                            (
                                rootFolder: Path.GetDirectoryName(path),
                                hostName: Path.GetFileName(path),
                                    defaultPage: Path.GetFileName(path)
                            )

                        });
                        path = "localfolder://" + Path.GetFileName(path);

                        Cef.Initialize(settings);
                        chromeBrowser = new ChromiumWebBrowser(path);
                    }
                    break;
                case LinkType.online:
                    {
                        Cef.Initialize(settings);
                        chromeBrowser = new ChromiumWebBrowser(path);
                    }
                    break;
                case LinkType.standalone:
                    {
                        Cef.Initialize(settings);
                        chromeBrowser = new ChromiumWebBrowser(path)
                        {
                            DownloadHandler = new DownloadHandler()
                        };
                    }
                    break;
            }

            chromeBrowser.MenuHandler = new CefMenuHandler();

            chromeBrowser.LifeSpanHandler = new CefPopUpHandle();

            Controls.Add(chromeBrowser);
            chromeBrowser.Dock = DockStyle.Fill;

            chromeBrowser.IsBrowserInitializedChanged += ChromeBrowser_IsBrowserInitializedChanged1;
            chromeBrowser.LoadingStateChanged += ChromeBrowser_LoadingStateChanged;
            chromeBrowser.LoadError += ChromeBrowser_LoadError;
            chromeBrowser.TitleChanged += ChromeBrowser_TitleChanged;
            chromeBrowser.ConsoleMessage += ChromeBrowser_ConsoleMessage;
        }

        private void SysAudio_AudioData(object sender, float[] fftBuffer)
        {
            if (suspendJsMsg)
                return;

            try
            {
                if (chromeBrowser.CanExecuteJavascriptInMainFrame)
                {
                    if (fftBuffer != null)
                    {
                        ExecuteScriptAsync(chromeBrowser, "AudioListener", fftBuffer);
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void SysMonitor_HardwareUsage(object sender, HWUsageMonitorEventArgs e)
        {
            if (suspendJsMsg)
                return;

            try
            {
                if (chromeBrowser.CanExecuteJavascriptInMainFrame)
                {
                    chromeBrowser.ExecuteScriptAsync("IPCSystemInformation", JsonConvert.SerializeObject(e, Formatting.Indented));
                }
            }
            catch
            {
                
            }
        }

        private void ChromeBrowser_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            WriteToParent(new IPCMessageConsole()
            {
                Category = ConsoleMessageType.console,
                Message = e.Message
            });
        }

        private void ChromeBrowser_TitleChanged(object sender, TitleChangedEventArgs e)
        {
            Invoke((MethodInvoker)(() => Text = e.Title));
        }

        private void ChromeBrowser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (e.IsLoading)
            {
                return;
            }

            if (propertyData != null)
            {
                RestoreLivelyPropertySettings();
            }
            WriteToParent(new IPCMessageWallpaperLoaded() { Success = true });
        }

        private void RestoreLivelyPropertySettings()
        {
            try
            {
                if (chromeBrowser.CanExecuteJavascriptInMainFrame)
                {
                    foreach (var item in propertyData)
                    {
                        string uiElementType = item.Value["type"].ToString();
                        if (!uiElementType.Equals("button", StringComparison.OrdinalIgnoreCase) && !uiElementType.Equals("label", StringComparison.OrdinalIgnoreCase))
                        {
                            if (uiElementType.Equals("dropdown", StringComparison.OrdinalIgnoreCase))
                            {
                                chromeBrowser.ExecuteScriptAsync("LateCatPropertyListener", item.Key, (int)item.Value["value"]);
                            }
                            else if (uiElementType.Equals("slider", StringComparison.OrdinalIgnoreCase))
                            {
                                chromeBrowser.ExecuteScriptAsync("LateCatPropertyListener", item.Key, (double)item.Value["value"]);
                            }
                            else if (uiElementType.Equals("folderDropdown", StringComparison.OrdinalIgnoreCase))
                            {
                                var filePath = Path.Combine(Path.GetDirectoryName(htmlPath), item.Value["folder"].ToString(), item.Value["value"].ToString());
                                if (File.Exists(filePath))
                                {
                                    chromeBrowser.ExecuteScriptAsync("LateCatPropertyListener",
                                    item.Key,
                                    Path.Combine(item.Value["folder"].ToString(), item.Value["value"].ToString()));
                                }
                                else
                                {
                                    chromeBrowser.ExecuteScriptAsync("LateCatPropertyListener",
                                    item.Key,
                                    null);
                                }
                            }
                            else if (uiElementType.Equals("checkbox", StringComparison.OrdinalIgnoreCase))
                            {
                                chromeBrowser.ExecuteScriptAsync("LateCatPropertyListener", item.Key, (bool)item.Value["value"]);
                            }
                            else if (uiElementType.Equals("color", StringComparison.OrdinalIgnoreCase) || uiElementType.Equals("textbox", StringComparison.OrdinalIgnoreCase))
                            {
                                chromeBrowser.ExecuteScriptAsync("LateCatPropertyListener", item.Key, (string)item.Value["value"]);
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private void ChromeBrowser_IsBrowserInitializedChanged1(object sender, EventArgs e)
        {
            WriteToParent(new IPCMessageHwnd()
            {
                Hwnd = chromeBrowser.GetBrowser().GetHost().GetWindowHandle().ToInt32()
            });
        }

        private void ChromeBrowser_LoadError(object sender, LoadErrorEventArgs e)
        {
            Debug.WriteLine("Error Loading Page:-" + e.ErrorText);
            if (linkType == LinkType.local || e.ErrorCode == CefErrorCode.Aborted || e.ErrorCode == (CefErrorCode)(-27))
            {
                return;
            }
            chromeBrowser.LoadHtml(@"<head> <meta charset=""utf - 8""> <title>Error</title>  <style>
            * { line-height: 1.2; margin: 0; } html { display: table; font-family: sans-serif; height: 100%; text-align: center; width: 100%; } body { background-color: #252525; display:
            table-cell; vertical-align: middle; margin: 2em auto; } h1 { color: #e5e5e5; font-size: 2em; font-weight: 400; } p { color: #cccccc; margin: 0 auto; width: 280px; } .url{color: #e5e5e5; position: absolute; margin: 16px; right: 0; top: 0; } @media only
            screen and (max-width: 280px) { body, p { width: 95%; } h1 { font-size: 1.5em; margin: 0 0 0.3em; } } </style></head><body><div class=""url"">" + originalUrl + "</div> <h1>Unable to load webpage :'(</h1> <p>" + e.ErrorText + "</p></body></html>");
        }

        #endregion //cef

        #region helpers

        public async Task CaptureScreenshot(ChromiumWebBrowser chromeBrowser, string filePath, ScreenshotFormat format)
        {
            DevToolsExtensions.CaptureFormat captureFormat = DevToolsExtensions.CaptureFormat.Png;
            switch (format)
            {
                case ScreenshotFormat.jpeg:
                    captureFormat = DevToolsExtensions.CaptureFormat.JPEG;
                    break;
                case ScreenshotFormat.png:
                    captureFormat = DevToolsExtensions.CaptureFormat.Png;
                    break;
                case ScreenshotFormat.webp:
                    captureFormat = DevToolsExtensions.CaptureFormat.Webp;
                    break;
                case ScreenshotFormat.bmp:
                    captureFormat = DevToolsExtensions.CaptureFormat.Png;
                    break;
            }

            var imageBytes = await DevToolsExtensions.CaptureScreenShotAsPng(chromeBrowser, captureFormat);

            switch (format)
            {
                case ScreenshotFormat.jpeg:
                case ScreenshotFormat.png:
                case ScreenshotFormat.webp:
                    {
                        File.WriteAllBytes(filePath, imageBytes);
                    }
                    break;
                case ScreenshotFormat.bmp:
                    {
                        using var ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
                        using var image = Image.FromStream(ms, true);
                        image.Save(filePath, ImageFormat.Bmp);
                    }
                    break;
            }
        }


        void ExecuteScriptAsync(ChromiumWebBrowser chromeBrowser, string methodName, float[] args)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(methodName);
            stringBuilder.Append("([");

            for (int i = 0; i < args.Length; i++)
            {
                stringBuilder.Append(args[i]);
                stringBuilder.Append(',');
            }

            stringBuilder.Remove(stringBuilder.Length - 2, 2);

            stringBuilder.Append("]);");
            var script = stringBuilder.ToString();

            chromeBrowser.ExecuteScriptAsync(script);
        }

        #endregion //helpers
    }
}
