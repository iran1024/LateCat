﻿using CommandLine;

namespace LateCat.CefPlayer
{
    public class StartArgs
    {
        [Option("url",
        Required = true,
        HelpText = "The url/html-file to load.")]
        public string Url { get; set; }

        [Option("property",
        Required = false,
        Default = null,
        HelpText = "Properties.info filepath (.late/data).")]
        public string Properties { get; set; }

        [Option("type",
        Required = true,
        HelpText = "LinkType class.")]
        public string Type { get; set; }

        [Option("display",
        Required = true,
        HelpText = "Wallpaper running display.")]
        public string DisplayDevice { get; set; }

        [Option("geometry",
        Required = false,
        HelpText = "Window size (WxH).")]
        public string Geometry { get; set; }

        [Option("audio",
        Default = false,
        HelpText = "Analyse system audio(visualiser data.)")]
        public bool AudioAnalyse { get; set; }

        [Option("debug",
        Required = false,
        HelpText = "Debugging port")]
        public string DebugPort { get; set; }

        [Option("cache",
        Required = false,
        HelpText = "disk cache path")]
        public string CachePath { get; set; }

        [Option("volume",
        Required = false,
        Default = 100,
        HelpText = "Audio volume")]
        public int Volume { get; set; }

        [Option("system-information",
        Default = false,
        Required = false,
        HelpText = "hw monitor api")]
        public bool SysInfo { get; set; }

        [Option("verbose-log",
        Required = false,
        HelpText = "Verbose Logging")]
        public bool VerboseLog { get; set; }
    }
}
