using LateCat.PoseidonEngine.Core;
using System;

namespace LateCat.PoseidonEngine.Abstractions
{
    internal interface IPlayback
    {
        void Start();
        void Stop();
        PlaybackState WallpaperPlaybackState { get; set; }

        event EventHandler<PlaybackState> PlaybackStateChanged;
    }
}
