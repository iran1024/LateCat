using LibVLCSharp.Shared;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace LateCat.LibVLCPlayer
{
    public partial class MainWindow : Window
    {
        private bool _mediaReady = false;
        private LibVLC _libVLC;
        private MediaPlayer _mediaPlayer;
        private Media _media;
        private readonly string _filePath;
        private readonly bool _isStream;
        private float _vidPosition;

        public MainWindow(string[] args)
        {
            InitializeComponent();
            _filePath = args[0];
            _isStream = false;
            VideoView.Loaded += VideoView_Loaded;
        }

        async void VideoView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Core.Initialize();

                _libVLC = new LibVLC();
                _mediaPlayer = new MediaPlayer(_libVLC)
                {
                    AspectRatio = "Fill",
                    EnableHardwareDecoding = true,
                    Volume = 0
                };
                _mediaPlayer.EndReached += _mediaPlayer_EndReached;
                _mediaPlayer.EncounteredError += _mediaPlayer_EncounteredError;
                VideoView.MediaPlayer = _mediaPlayer;

                if (_isStream)
                {
                    _media = new Media(_libVLC, _filePath, FromType.FromLocation);

                    await _media.Parse(MediaParseOptions.ParseNetwork);

                    _mediaPlayer.Play(_media.SubItems.First());
                }
                else
                {
                    _media = new Media(_libVLC, _filePath, FromType.FromPath);
                    _mediaPlayer.Play(_media);
                }
                _mediaReady = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                ListenToParent();
            }
        }

        private void _mediaPlayer_EndReached(object? sender, EventArgs e)
        {
            if (_mediaPlayer == null)
                return;

            if (_isStream)
            {
                ThreadPool.QueueUserWorkItem(_ => _mediaPlayer.Play(_media.SubItems.First()));
            }
            else
            {
                ThreadPool.QueueUserWorkItem(_ => _mediaPlayer.Play(_media));
            }
        }

        private void PausePlayer()
        {
            if (_mediaPlayer == null)
                return;

            if (_mediaPlayer.IsPlaying && _mediaReady)
            {
                _vidPosition = _mediaPlayer.Position;
                _mediaPlayer.Stop();
            }
        }

        private void PlayMedia()
        {
            if (_mediaPlayer == null)
                return;

            if (_mediaReady && !_mediaPlayer.IsPlaying)
            {
                _mediaPlayer.Play();
                _mediaPlayer.Position = _vidPosition;
            }
        }

        private void StopPlayer()
        {
            if (_mediaPlayer == null)
                return;

            if (_mediaReady)
            {
                _mediaPlayer.Stop();
            }
        }

        private void SetVolume(int volume)
        {
            if (_mediaReady)
            {
                _mediaPlayer.Volume = volume;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                _mediaReady = false;
                _mediaPlayer.EndReached -= _mediaPlayer_EndReached;
                _mediaPlayer.Dispose();
                _libVLC.Dispose();
                _media.Dispose();
            }
            catch { }
        }

        private void _mediaPlayer_EncounteredError(object? sender, EventArgs e)
        {
            Console.WriteLine("Media playback Error");
        }

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

        public async void ListenToParent()
        {
            try
            {
                await Task.Run(async () =>
                {
                    while (true)
                    {
                        var text = await Console.In.ReadLineAsync();

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
                    }
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                Application.Current.Shutdown();
            }
        }

        #region helpers

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(HandleRef hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(HandleRef hWnd, int nIndex, IntPtr dwNewLong);

        private const uint WS_EX_NOACTIVATE = 0x08000000;
        private const uint WS_EX_TOOLWINDOW = 0x00000080;

        private static IntPtr SetWindowLongPtr(HandleRef hWnd, int nIndex, IntPtr dwNewLong)
        {

            if (IntPtr.Size == 8)
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            else
                return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));

        }

        #endregion //helpers
    }
}
