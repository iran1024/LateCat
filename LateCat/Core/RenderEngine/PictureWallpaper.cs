using LateCat.PoseidonEngine.Abstractions;
using LateCat.PoseidonEngine.Core;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LateCat.Core
{
    class PictureWallpaper : IWallpaper
    {
        public event EventHandler<WindowInitializedArgs> WindowInitialized;

        private readonly DesktopWallpaperPosition _desktopWallpaperScaler;
        private readonly INativeWallpaper _nativeWallpaper;
        private readonly IWallpaperMetadata _metadata;
        private string _systemWallpaperPath;
        private IMonitor _monitor;

        public bool IsLoaded => true;

        public WallpaperType WallpaperType => WallpaperType.Picture;

        public IWallpaperMetadata Metadata => _metadata;

        public IntPtr Handle => IntPtr.Zero;

        public IntPtr InputHandle => IntPtr.Zero;

        public Process Proc => null;

        public IMonitor Monitor { get => _monitor; set => _monitor = value; }

        public string PropertyCopyPath => null;

        private readonly WallpaperArrangement arrangement;

        public PictureWallpaper(string filePath, IWallpaperMetadata metadata, IMonitor monitor, WallpaperArrangement arrangement, WallpaperScaler scaler = WallpaperScaler.Fill)
        {
            this.arrangement = arrangement;


            _nativeWallpaper = (INativeWallpaper)new NativeWallpaperInterop();

            _systemWallpaperPath = _nativeWallpaper.GetWallpaper(monitor.DeviceId);
            _desktopWallpaperScaler = DesktopWallpaperPosition.Fill;
            switch (scaler)
            {
                case WallpaperScaler.None:
                    _desktopWallpaperScaler = DesktopWallpaperPosition.Center;
                    break;
                case WallpaperScaler.Fill:
                    _desktopWallpaperScaler = DesktopWallpaperPosition.Stretch;
                    break;
                case WallpaperScaler.Uniform:
                    _desktopWallpaperScaler = DesktopWallpaperPosition.Fit;
                    break;
                case WallpaperScaler.UniformToFill:
                    _desktopWallpaperScaler = DesktopWallpaperPosition.Fill;
                    break;
            }

            _monitor = monitor;
            _metadata = metadata;
        }

        public void Close()
        {
            Terminate();
        }

        public void Pause()
        {
            //nothing
        }

        public void Play()
        {
            //nothing
        }

        public Task Capture(string filePath)
        {
            throw new NotImplementedException();
        }

        public void SendMessage(string msg)
        {
            //nothing
        }

        public void SetPlaybackPos(float pos, PlaybackPosType type)
        {
            //nothing
        }

        public void SetMonitor(PoseidonMonitor monitor)
        {
            _monitor = monitor;
        }

        public void SetVolume(int volume)
        {
            //nothing
        }

        public void Show()
        {
            //desktop.Enable();
            _nativeWallpaper.SetPosition(arrangement == WallpaperArrangement.Span ? DesktopWallpaperPosition.Span : _desktopWallpaperScaler);
            _nativeWallpaper.SetWallpaper(arrangement == WallpaperArrangement.Span ? null : _monitor.DeviceId, _metadata.FilePath);
        }

        public void Stop()
        {
            //nothing
        }

        public void Terminate()
        {
            //restoring original wallpaper.
            _nativeWallpaper.SetWallpaper(_monitor.DeviceId, _systemWallpaperPath);
        }

        public void SendMessage(IPCMessage obj)
        {
            //todo
        }
    }
}
