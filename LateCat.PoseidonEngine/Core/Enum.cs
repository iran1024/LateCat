﻿using System.ComponentModel;

namespace LateCat.PoseidonEngine.Core
{
    public enum IndexState : byte
    {
        Normal,
        FrontEnd,
        BackEnd
    }

    public enum WallpaperType
    {
        [Description("Application")]
        App,
        [Description("Webpage")]
        Web,
        [Description("Webpage Audio Visualiser")]
        WebAudio,
        [Description("Webpage Link")]
        Url,
        [Description("Bizhawk Emulator")]
        Bizhawk,
        [Description("Unity Game")]
        Unity,
        [Description("Godot Game")]
        Godot,
        [Description("Video")]
        Video,
        [Description("Animated Gif")]
        Gif,
        [Description("Unity Audio Visualiser")]
        UnityAudio,
        [Description("Video Streams")]
        VideoStream,
        [Description("Static picture")]
        Picture,
    }

    public enum MonitorIdentificationMode
    {
        DeviceName,
        DeviceId,
        MonitorLayout
    }

    public enum AudioPauseAlgorithm
    {
        None,
        Cursor,
        PrimaryMonitor,
    }

    public enum PerformanceStrategy
    {
        [Description("Pause")]
        Pause,
        [Description("Ignore")]
        Keep,
        [Description("Kill(Free Memory)")]
        Kill
    }

    public enum MonitorPauseStrategy
    {
        PerMonitor,
        All
    }

    public enum ProcessMonitorAlgorithm
    {
        Foreground,
        All,
        GameMode
    }

    public enum StreamQualitySuggestion
    {
        [Description("480p")]
        Medium,
        [Description("720p")]
        MediumHigh,
        [Description("1080p")]
        High,
        [Description("1080p+")]
        Highest
    }

    public enum WallpaperArrangement
    {
        [Description("Per Display")]
        Per,
        [Description("Span Across All Display(s)")]
        Span,
        [Description("Same wp for all Display(s)")]
        Duplicate
    }

    public enum InputForwardMode
    {
        Off,
        Mouse,
        MouseKeyboard,
    }

    public enum PlaybackState
    {
        [Description("Normal")]
        Play,
        [Description("All Wallpapers Paused")]
        Paused
    }

    public enum MediaPlayerType
    {
        Wmf,
        LibVLC,
        LibVLCExt,
        LibMPV,
        LibMPVExt,
        MPV,
        VLC
    }

    public enum GifPlayer
    {
        LibMPVExt,
        MPV
    }

    public enum PicturePlayer
    {
        Picture
    }

    public enum WebBrowser
    {
        Cef,
        WebView2
    }

    public enum WallpaperScaler
    {
        None,
        Fill,
        Uniform,
        UniformToFill,
        Auto
    }

    public enum TaskbarTheme
    {
        [Description("System default.")]
        None,
        [Description("Fully transparent.")]
        Clear,
        [Description("Blurred.")]
        Blur,
        [Description("Fluent design.")]
        Fluent,
        [Description("User defined color.")]
        Color,
        [Description("Wallpaper color avg.")]
        Wallpaper,
        [Description("Wallpaper color avg fluet style.")]
        WallpaperFluent,
    }

    public enum WallpaperProcessStatus
    {
        [Description("Converting to mp4")]
        VideoConvert,
        [Description("To be added to library")]
        Processing,
        Installing,
        Downloading,
        [Description("Ready to be used")]
        Ready,
        CmdImport,
        MultiImport,
        Edit
    }

    public enum PlaybackPosType
    {
        Absolute,
        Relative
    }

    public enum DesktopSlideshowOptions
    {
        ShuffleImages = 0x01,
    }

    public enum DesktopSlideshowState
    {
        Enabled = 0x01,
        Slideshow = 0x02,
        DisabledByRemoteSession = 0x04,
    }

    public enum DesktopSlideshowDirection
    {
        Forward = 0,
        Backward = 1,
    }

    public enum DesktopWallpaperPosition
    {
        Center = 0,
        Tile = 1,
        Stretch = 2,
        Fit = 3,
        Fill = 4,
        Span = 5,
    }
}
