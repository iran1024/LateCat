using LateCat.PoseidonEngine.Core;
using System.Windows.Media;

namespace LateCat.PoseidonEngine.Abstractions
{
    public interface ISettings
    {
        bool Startup { get; set; }
        bool ControlPanelOpened { get; set; }
        PerformanceStrategy OtherAppFocus { get; set; }
        PerformanceStrategy OtherAppFullScreen { get; set; }
        PerformanceStrategy InBatteryMode { get; set; }
        PerformanceStrategy InRemoteDesktop { get; set; }
        PerformanceStrategy PowerSaveMode { get; set; }
        MonitorPauseStrategy MonitorPauseStrategy { get; set; }
        ProcessMonitorAlgorithm ProcessMonitorAlgorithm { get; set; }
        Stretch Scaler { get; set; }
        WallpaperArrangement WallpaperArrangement { get; set; }
        int ProcessTimerInterval { get; set; }
        InputForwardMode InputForward { get; set; }
        bool MouseInputMoveAlways { get; set; }
        int TileSize { get; set; }
        MediaPlayerType VideoPlayer { get; set; }
        bool VideoPlayerHwAccel { get; set; }
        GifPlayer GifPlayer { get; set; }
        WebBrowser WebBrowser { get; set; }
        IMonitor SelectedMonitor { get; set; }        
        string WallpaperDir { get; set; }
        string WallpaperTempDir { get; set; }
        string WallpaperDataDir { get; set; }
        bool WallpaperDirMoveExistingWallpaperNewDir { get; set; }
        bool AutoDetectOnlineStreams { get; set; }
        StreamQualitySuggestion StreamQuality { get; set; }
        int AudioVolumeGlobal { get; set; }
        bool AudioOnlyOnDesktop { get; set; }
        WallpaperScaler WallpaperScaling { get; set; }
        bool CefDiskCache { get; set; }
        bool DesktopAutoWallpaper { get; set; }
        TaskbarTheme SystemTaskbarTheme { get; set; }
    }
}
