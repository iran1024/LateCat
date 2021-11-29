using LateCat.PoseidonEngine.Abstractions;
using LateCat.PoseidonEngine.Core;
using System;
using System.Windows.Media;

namespace LateCat.Models
{
    [Serializable]
    internal class Settings : ISettings
    {
        public bool Startup { get; set; }
        public bool ControlPanelOpened { get; set; }
        public PerformanceStrategy OtherAppFocus { get; set; }
        public PerformanceStrategy OtherAppFullScreen { get; set; }
        public PerformanceStrategy InBatteryMode { get; set; }
        public PerformanceStrategy InRemoteDesktop { get; set; }
        public PerformanceStrategy PowerSaveMode { get; set; }
        public MonitorPauseStrategy MonitorPauseStrategy { get; set; }
        public ProcessMonitorAlgorithm ProcessMonitorAlgorithm { get; set; }
        public Stretch Scaler { get; set; }
        public WallpaperArrangement WallpaperArrangement { get; set; }
        public int ProcessTimerInterval { get; set; }
        public InputForwardMode InputForward { get; set; }
        public bool MouseInputMoveAlways { get; set; }
        public int TileSize { get; set; }
        public MediaPlayerType VideoPlayer { get; set; }
        public bool VideoPlayerHwAccel { get; set; }
        public GifPlayer GifPlayer { get; set; }
        public WebBrowser WebBrowser { get; set; }
        public IMonitor SelectedMonitor { get; set; }        
        public string WallpaperDir { get; set; }
        public string WallpaperTempDir { get; set; }
        public string WallpaperDataDir { get; set; }
        public bool WallpaperDirMoveExistingWallpaperNewDir { get; set; }
        public bool AutoDetectOnlineStreams { get; set; }
        public StreamQualitySuggestion StreamQuality { get; set; }
        public int AudioVolumeGlobal { get; set; }
        public bool AudioOnlyOnDesktop { get; set; }
        public WallpaperScaler WallpaperScaling { get; set; }
        public bool CefDiskCache { get; set; }
        public bool DesktopAutoWallpaper { get; set; }
        public TaskbarTheme SystemTaskbarTheme { get; set; }

        public Settings()
        {
            ProcessMonitorAlgorithm = ProcessMonitorAlgorithm.Foreground;
            WallpaperArrangement = WallpaperArrangement.Per;
            Startup = true;
            ControlPanelOpened = false;
            OtherAppFocus = PerformanceStrategy.Keep;
            OtherAppFullScreen = PerformanceStrategy.Pause;
            InBatteryMode = PerformanceStrategy.Keep;
            VideoPlayer = MediaPlayerType.MPV;
            VideoPlayerHwAccel = true;
            WebBrowser = WebBrowser.WebView2;
            GifPlayer = GifPlayer.MPV;

            ProcessTimerInterval = 500;
            StreamQuality = StreamQualitySuggestion.High;

            Scaler = Stretch.Fill;

            InputForward = InputForwardMode.Mouse;
            MouseInputMoveAlways = true;

            TileSize = 1;
            SelectedMonitor = MonitorHelper.GetPrimaryMonitor();            
            WallpaperDir = PoseidonEngine.Constants.Paths.WallpaperDir;
            WallpaperTempDir = PoseidonEngine.Constants.Paths.WallpaperTempDir;
            WallpaperDataDir = PoseidonEngine.Constants.Paths.WallpaperDataDir;
            WallpaperDirMoveExistingWallpaperNewDir = false;
            AutoDetectOnlineStreams = true;
            AudioVolumeGlobal = 75;
            AudioOnlyOnDesktop = true;
            WallpaperScaling = WallpaperScaler.Fill;
            CefDiskCache = false;
            InRemoteDesktop = PerformanceStrategy.Pause;
            PowerSaveMode = PerformanceStrategy.Keep;

            DesktopAutoWallpaper = false;
            SystemTaskbarTheme = TaskbarTheme.None;
        }
    }
}
