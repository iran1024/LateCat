using LateCat.PoseidonEngine.Core;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LateCat.PoseidonEngine.Abstractions
{
    public interface IWallpaper
    {
        IMonitor Monitor { get; set; }

        string PropertyCopyPath { get; }

        bool IsLoaded { get; }

        WallpaperType WallpaperType { get; }

        IWallpaperMetadata Metadata { get; }

        IntPtr Handle { get; }

        IntPtr InputHandle { get; }

        Process Proc { get; }

        void SendMessage(string msg);

        void SendMessage(IPCMessage obj);

        void Show();

        void Pause();

        void Play();

        void Stop();

        void Close();

        void Terminate();

        void SetVolume(int volume);

        void SetPlaybackPos(float pos, PlaybackPosType type);

        Task Capture(string filePath);

        event EventHandler<WindowInitializedArgs> WindowInitialized;
    }

    public class WindowInitializedArgs : EventArgs
    {
        public bool Success { get; set; }

        public Exception Error { get; set; }

        public string Message { get; set; }
    }
}
