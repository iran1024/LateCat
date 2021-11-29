using LateCat.Core;
using LateCat.PoseidonEngine.Abstractions;
using LateCat.PoseidonEngine.Core;
using System;

namespace LateCat.Factory
{
    internal class WallpaperFactory : IWallpaperFactory
    {
        private readonly IPropertyFactory _propertyFactory;

        public WallpaperFactory(IPropertyFactory propertyFactory)
        {
            _propertyFactory = propertyFactory;
        }

        public IWallpaper CreateWallpaper(IWallpaperMetadata metadata, IMonitor monitor, ISettingsService settings)
        {
            switch (metadata.WallpaperInfo.Type)
            {
                case WallpaperType.Web:
                case WallpaperType.WebAudio:
                case WallpaperType.Url:
                    {
                        return GetWallpaperFromUrl(metadata, monitor, settings);
                    }
                case WallpaperType.Video:
                    {
                        return GetWallpaperFromVideo(metadata, monitor, settings);
                    }
                case WallpaperType.Gif:
                case WallpaperType.Picture:
                    {
                        return GetWallpaperFromImage(metadata, monitor, settings);
                    }
                default:
                    {
                        throw new NotImplementedException();
                    }
            }
        }

        private IWallpaper GetWallpaperFromUrl(IWallpaperMetadata metadata, IMonitor monitor, ISettingsService settings)
        {
            return settings.Settings.WebBrowser switch
            {
                WebBrowser.Cef => new WebProcess(metadata.FilePath,
                                        metadata,
                                        monitor,
                                        _propertyFactory.CreatePropertyFolder(metadata, monitor, settings.Settings.WallpaperArrangement),
                                        settings.Settings.CefDiskCache,
                                        settings.Settings.AudioVolumeGlobal),
                WebBrowser.WebView2 => new WebEdge(metadata.FilePath, metadata, monitor, _propertyFactory.CreatePropertyFolder(metadata, monitor, settings.Settings.WallpaperArrangement)),
                _ => throw new NotImplementedException(),
            };
        }

        private IWallpaper GetWallpaperFromVideo(IWallpaperMetadata metadata, IMonitor monitor, ISettingsService settings)
        {
            return settings.Settings.VideoPlayer switch
            {
                MediaPlayerType.Wmf => new VideoPlayerWPF(metadata.FilePath, metadata, monitor, settings.Settings.WallpaperScaling),
                MediaPlayerType.LibVLC => throw new NotImplementedException(),
                MediaPlayerType.LibVLCExt => new VideoPlayerVLCExt(metadata.FilePath, metadata, monitor),
                MediaPlayerType.LibMPV => throw new NotImplementedException(),
                MediaPlayerType.LibMPVExt => new VideoPlayerMPVExt(metadata.FilePath, metadata, monitor, _propertyFactory.CreatePropertyFolder(metadata, monitor, settings.Settings.WallpaperArrangement), settings.Settings.WallpaperScaling),
                MediaPlayerType.MPV => new VideoMpvPlayer(metadata.FilePath, metadata, monitor, _propertyFactory.CreatePropertyFolder(metadata, monitor, settings.Settings.WallpaperArrangement), settings.Settings.WallpaperScaling, settings.Settings.VideoPlayerHwAccel),
                MediaPlayerType.VLC => new VideoVlcPlayer(metadata.FilePath, metadata, monitor, settings.Settings.WallpaperScaling, settings.Settings.VideoPlayerHwAccel),
                _ => throw new NotImplementedException(),
            };
        }

        private IWallpaper GetWallpaperFromImage(IWallpaperMetadata metadata, IMonitor monitor, ISettingsService settings)
        {
            return settings.Settings.GifPlayer switch
            {
                GifPlayer.MPV => new VideoMpvPlayer(metadata.FilePath, metadata, monitor, _propertyFactory.CreatePropertyFolder(metadata, monitor, settings.Settings.WallpaperArrangement), settings.Settings.WallpaperScaling, settings.Settings.VideoPlayerHwAccel),
                GifPlayer.LibMPVExt => new VideoPlayerMPVExt(metadata.FilePath, metadata, monitor, _propertyFactory.CreatePropertyFolder(metadata, monitor, settings.Settings.WallpaperArrangement), settings.Settings.WallpaperScaling),
                _ => throw new NotImplementedException()
            };
        }
    }

    public class DepreciatedException : Exception
    {
        public DepreciatedException()
        {
        }

        public DepreciatedException(string message)
            : base(message)
        {
        }

        public DepreciatedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class PluginNotFoundException : Exception
    {
        public PluginNotFoundException()
        {
        }

        public PluginNotFoundException(string message)
            : base(message)
        {
        }

        public PluginNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}