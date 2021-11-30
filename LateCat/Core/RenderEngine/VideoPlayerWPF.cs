using LateCat.PoseidonEngine.Abstractions;
using LateCat.PoseidonEngine.Core;
using LateCat.Views;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Threading;

namespace LateCat.Core
{
    public class VideoPlayerWPF : IWallpaper
    {
        private IntPtr _hWnd;
        private readonly MediaElementWPF _render;
        private readonly IWallpaperMetadata _metadata;
        private IMonitor _monitor;

        public bool IsLoaded => _render?.IsActive == true;

        public WallpaperType WallpaperType => _metadata.WallpaperInfo.Type;

        public IWallpaperMetadata Metadata => _metadata;

        public IntPtr Handle => _hWnd;

        public IntPtr InputHandle => IntPtr.Zero;

        public Process Proc => null;

        public IMonitor Monitor { get => _monitor; set => _monitor = value; }

        public string PropertyCopyPath => null;

        public event EventHandler<WindowInitializedArgs> WindowInitialized;

        public VideoPlayerWPF(string filePath, IWallpaperMetadata model, IMonitor display, WallpaperScaler scaler = WallpaperScaler.Fill)
        {
            _render = new MediaElementWPF(filePath, scaler == WallpaperScaler.Auto ? WallpaperScaler.Uniform : scaler);
            _metadata = model;
            _monitor = display;
        }

        public void Play()
        {
            _render.PlayMedia();
        }

        public void Pause()
        {
            _render.PausePlayer();
        }

        public void Stop()
        {
            _render.StopPlayer();
        }

        public void Close()
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new ThreadStart(delegate
            {
                _render.Close();
            }));
        }

        public void Show()
        {
            if (_render != null)
            {
                _render.Closed += Player_Closed;
                _render.Show();
                _hWnd = new WindowInteropHelper(_render).Handle;
                WindowInitialized?.Invoke(this, new WindowInitializedArgs() { Success = true, Error = null });
            }
        }

        private void Player_Closed(object sender, EventArgs e)
        {
            DesktopUtil.RefreshDesktop(Program.OriginalDesktopWallpaperPath);
        }

        public void SendMessage(string msg)
        {
            //todo
        }

        public void Terminate()
        {
            Close();
        }

        public void SetVolume(int volume)
        {
            _render.SetVolume(volume);
        }

        public void SetPlaybackPos(float pos, PlaybackPosType type)
        {
            //todo
        }

        public Task Capture(string filePath)
        {
            throw new NotImplementedException();
        }

        public void SendMessage(IPCMessage obj)
        {
            //todo
        }
    }
}
