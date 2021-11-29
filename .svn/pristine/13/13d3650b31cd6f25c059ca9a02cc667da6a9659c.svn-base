using CommandLine;
using Mpv.NET.Player;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Application = System.Windows.Application;

namespace LateCat.LibMPVPlayer
{
    public partial class MainWindow : Window
    {
        private MpvPlayer _player;
        private string _propertiesPath;
        private JObject _propertiesData;

        public MainWindow(string[] args)
        {
            InitializeComponent();
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(RunOptions)
                .WithNotParsed(HandleParseError);
        }

        #region cmdline

        class Options
        {
            [Option("path",
            Required = true,
            HelpText = "The file/video stream path.")]
            public string FilePath { get; set; }

            [Option("stream",
            Required = false,
            Default = 0,
            HelpText = "ytdl stream quality.")]
            public int StreamQuality { get; set; }

            [Option("stretch",
            Required = false,
            Default = 0,
            HelpText = "Video Scaling algorithm.")]
            public int StretchMode { get; set; }

            [Option("datadir",
            Required = false,
            HelpText = "App data directory")]
            public string AppDataDir { get; set; }

            [Option("property",
            Required = false,
            Default = null,
            HelpText = "Properties.info filepath (WallpaperDataDir).")]
            public string Properties { get; set; }
        }

        private void RunOptions(Options opts)
        {
            try
            {
                _player = new MpvPlayer(PlayerHost.Handle)
                {
                    Loop = true,
                    Volume = 0,
                };

                _player.MediaError += Player_MediaError1;

                if (File.Exists(Path.Combine(opts.AppDataDir, "mpv", "mpv.conf")))
                {
                    Console.WriteLine("Init custom mpv.conf");
                    _player.API.LoadConfigFile(Path.Combine(opts.AppDataDir, "mpv", "mpv.conf"));
                }
                else
                {
                    _player.API.SetPropertyString("hwdec", "auto");

                    _player.EnableYouTubeDl();

                    YouTubeDlVideoQuality quality = YouTubeDlVideoQuality.Highest;
                    try
                    {
                        quality = (YouTubeDlVideoQuality)Enum.GetValues(typeof(YouTubeDlVideoQuality)).GetValue(opts.StreamQuality)!;
                    }
                    catch
                    {

                    }

                    _player.YouTubeDlVideoQuality = quality;

                    var stretch = (Stretch)opts.StretchMode;

                    switch (stretch)
                    {
                        case Stretch.None:
                            _player.API.SetPropertyString("video-unscaled", "yes");
                            break;
                        case Stretch.Fill:
                            _player.API.SetPropertyString("keepaspect", "no");
                            break;
                        case Stretch.Uniform:
                            _player.API.SetPropertyString("keepaspect", "yes");
                            break;
                        case Stretch.UniformToFill:
                            _player.API.SetPropertyString("panscan", "1.0");
                            break;
                        default:
                            _player.API.SetPropertyString("keepaspect", "no");
                            break;
                    }
                    if (Path.GetExtension(opts.FilePath).Equals(".gif", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("检测到Gif，正在切换到整数定标器。");
                        _player.API.SetPropertyString("scale", "nearest");
                    }
                }

                try
                {
                    if (!string.IsNullOrEmpty(opts.Properties))
                    {
                        _propertiesPath = opts.Properties;
                        _propertiesData = LoadProperties(_propertiesPath);
                        SetProperties(_propertiesData);
                    }
                }
                catch (Exception ie)
                {
                    Console.WriteLine("Error restoring properties=>" + ie.Message);
                }

                _player.Load(opts.FilePath);
                _player.Resume();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                ListenToParent();
            }
        }

        private void HandleParseError(IEnumerable<Error> errs)
        {
            Console.WriteLine("Error parsing cmdline args, Exiting!");
            Application.Current.Shutdown();
        }

        #endregion //cmdline

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IntPtr handle = new WindowInteropHelper(this).Handle;
            var styleNewWindowExtended =
                           WS_EX_NOACTIVATE |
                           WS_EX_TOOLWINDOW;

            SetWindowLongPtr(new HandleRef(null, handle), (-20), (IntPtr)styleNewWindowExtended);

            ShowInTaskbar = false;
            ShowInTaskbar = true;

            Console.WriteLine("HWND" + handle);
        }

        private void Player_MediaError1(object? sender, EventArgs e)
        {
            Console.WriteLine("Media playback Error");
        }

        public void PausePlayer()
        {
            if (_player != null)
            {
                _player.Pause();
            }
        }

        public void PlayMedia()
        {
            if (_player != null)
            {
                _player.Resume();
            }
        }

        public void StopPlayer()
        {
            if (_player != null)
            {
                _player.Stop();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_player != null)
            {
                _player.Dispose();
            }
        }

        private void SetVolume(int volume)
        {
            if (_player != null)
            {
                _player.Volume = volume;
            }
        }

        public async void ListenToParent()
        {
            try
            {
                await Task.Run(async () =>
                {
                    while (true)
                    {
                        string text = await Console.In.ReadLineAsync();
                        if (string.IsNullOrEmpty(text))
                        {
                            break;
                        }
                        else if (text.Equals("LateCat:vid-pause", StringComparison.OrdinalIgnoreCase))
                        {
                            PausePlayer();
                        }
                        else if (text.Equals("LateCat:vid-play", StringComparison.OrdinalIgnoreCase))
                        {
                            PlayMedia();
                        }
                        else if (text.Equals("LateCat:terminate", StringComparison.OrdinalIgnoreCase))
                        {
                            break;
                        }
                        else if (text.Contains("LateCat:vid-volume", StringComparison.OrdinalIgnoreCase))
                        {
                            var msg = text.Split(' ');
                            if (msg.Length < 2)
                                continue;

                            if (int.TryParse(msg[1], out int value))
                            {
                                SetVolume(value);
                            }
                        }
                        else if (text.Contains("LateCat:customise", StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                var msg = text.Split(' ');
                                if (msg.Length < 4)
                                    continue;

                                SetProperties(msg[1], msg[2], msg[3]);
                            }
                            catch (Exception ex1)
                            {
                                Console.WriteLine(ex1.Message);
                            }
                        }
                    }
                });
            }
            catch (Exception ex2)
            {
                Console.WriteLine(ex2.Message);
            }
            finally
            {
                Application.Current.Shutdown();
            }
        }

        private void SetProperties(string uiElement, string objectName, string msg)
        {
            if (uiElement.Equals("dropdown", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(msg, out int value))
                {
                    _player.API.SetPropertyString(objectName, (string)_propertiesData[objectName]["items"][value]);
                }
            }
            else if (uiElement.Equals("slider", StringComparison.OrdinalIgnoreCase))
            {
                if (double.TryParse(msg, out double value))
                {
                    _player.API.SetPropertyString(objectName, msg);
                }
            }
            else if (uiElement.Equals("checkbox", StringComparison.OrdinalIgnoreCase))
            {
                if (bool.TryParse(msg, out bool value))
                {
                    _player.API.SetPropertyString(objectName, value ? "yes" : "no");
                }
            }
            else if (uiElement.Equals("button", StringComparison.OrdinalIgnoreCase))
            {
                if (objectName.Equals("LateCat_default_settings_reload", StringComparison.OrdinalIgnoreCase))
                {
                    _propertiesData = LoadProperties(_propertiesPath);

                    SetProperties(_propertiesData);
                }
                else
                {

                }
            }
        }

        private void SetProperties(JObject properties)
        {
            foreach (var item in properties)
            {
                var uiElement = item.Value["type"].ToString();
                if (!uiElement.Equals("button", StringComparison.OrdinalIgnoreCase) && !uiElement.Equals("label", StringComparison.OrdinalIgnoreCase))
                {
                    if (uiElement.Equals("slider", StringComparison.OrdinalIgnoreCase))
                    {
                        _player.API.SetPropertyString(item.Key, (string)item.Value["value"]);
                    }
                    else if (uiElement.Equals("checkbox", StringComparison.OrdinalIgnoreCase))
                    {
                        _player.API.SetPropertyString(item.Key, ((bool)item.Value["value"]) ? "yes" : "no");
                    }
                    else if (uiElement.Equals("dropdown", StringComparison.OrdinalIgnoreCase))
                    {
                        _player.API.SetPropertyString(item.Key, (string)item.Value["items"][(int)item.Value["value"]]);
                    }
                }
            }
        }

        #region helpers

        public static JObject LoadProperties(string path)
        {
            var json = File.ReadAllText(path);
            return JObject.Parse(json);
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(HandleRef hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(HandleRef hWnd, int nIndex, IntPtr dwNewLong);

        private const uint WS_EX_NOACTIVATE = 0x08000000;
        private const uint WS_EX_TOOLWINDOW = 0x00000080;

        public static IntPtr SetWindowLongPtr(HandleRef hWnd, int nIndex, IntPtr dwNewLong)
        {

            if (IntPtr.Size == 8)
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            else
                return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));

        }

        #endregion //helpers
    }
}
