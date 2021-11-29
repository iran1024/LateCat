using LateCat.CefPlayer.Services;
using Newtonsoft.Json;

namespace LateCat.Cefplayer
{
    public enum MessageType
    {
        msg_hwnd,
        msg_console,
        msg_wploaded,
        msg_screenshot,
        cmd_reload,
        cmd_close,
        cmd_screenshot,
        cmd_suspend,
        cmd_resume,
        lsp_perfcntr,
        lsp_nowplaying,
        lp_slider,
        lp_textbox,
        lp_dropdown,
        lp_fdropdown,
        lp_button,
        lp_cpicker,
        lp_chekbox,
    }

    public enum ConsoleMessageType
    {
        log,
        error,
        console
    }

    public enum ScreenshotFormat
    {
        jpeg,
        png,
        webp,
        bmp
    }

    [Serializable]
    public abstract class IPCMessage
    {
        [JsonProperty(Order = -2)]
        public MessageType Type { get; }
        public IPCMessage(MessageType type)
        {
            Type = type;
        }
    }

    [Serializable]
    public class IPCMessageConsole : IPCMessage
    {
        public string Message { get; set; }
        public ConsoleMessageType Category { get; set; }
        public IPCMessageConsole() : base(MessageType.msg_console)
        {
        }
    }

    [Serializable]
    public class IPCMessageHwnd : IPCMessage
    {
        public long Hwnd { get; set; }
        public IPCMessageHwnd() : base(MessageType.msg_hwnd)
        {
        }
    }

    [Serializable]
    public class IPCMessageMonitorshot : IPCMessage
    {
        public string FileName { get; set; }
        public bool Success { get; set; }
        public IPCMessageMonitorshot() : base(MessageType.msg_screenshot)
        {
        }
    }

    [Serializable]
    public class IPCMessageWallpaperLoaded : IPCMessage
    {
        public bool Success { get; set; }
        public IPCMessageWallpaperLoaded() : base(MessageType.msg_wploaded)
        {
        }
    }

    [Serializable]
    public class IPCCloseCmd : IPCMessage
    {
        public IPCCloseCmd() : base(MessageType.cmd_close)
        {
        }
    }

    [Serializable]
    public class IPCReloadCmd : IPCMessage
    {
        public IPCReloadCmd() : base(MessageType.cmd_reload)
        {
        }
    }

    [Serializable]
    public class IPCMonitorshotCmd : IPCMessage
    {
        public ScreenshotFormat Format { get; set; }
        public string FilePath { get; set; }
        public uint Delay { get; set; }
        public IPCMonitorshotCmd() : base(MessageType.msg_screenshot)
        {
        }
    }

    [Serializable]
    public class IPCSystemInformation : IPCMessage
    {
        public HWUsageMonitorEventArgs Info { get; set; }
        public IPCSystemInformation() : base(MessageType.cmd_reload)
        {
        }
    }

    [Serializable]
    public class IPCSystemNowPlaying : IPCMessage
    {
        public NowPlayingEventArgs Info { get; set; }
        public IPCSystemNowPlaying() : base(MessageType.cmd_reload)
        {
        }
    }

    [Serializable]
    public class IPCSlider : IPCMessage
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public double Step { get; set; }
        public IPCSlider() : base(MessageType.lp_slider)
        {
        }
    }

    [Serializable]
    public class IPCTextBox : IPCMessage
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public IPCTextBox() : base(MessageType.lp_textbox)
        {
        }
    }

    [Serializable]
    public class IPCDropdown : IPCMessage
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public IPCDropdown() : base(MessageType.lp_dropdown)
        {
        }
    }

    [Serializable]
    public class IPCFolderDropdown : IPCMessage
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public IPCFolderDropdown() : base(MessageType.lp_fdropdown)
        {
        }
    }

    [Serializable]
    public class IPCCheckbox : IPCMessage
    {
        public string Name { get; set; }
        public bool Value { get; set; }
        public IPCCheckbox() : base(MessageType.lp_chekbox)
        {
        }
    }

    [Serializable]
    public class IPCColorPicker : IPCMessage
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public IPCColorPicker() : base(MessageType.lp_cpicker)
        {
        }
    }

    [Serializable]
    public class IPCButton : IPCMessage
    {
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public IPCButton() : base(MessageType.lp_button)
        {
        }
    }
}
