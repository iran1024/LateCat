using LateCat.Core;
using LateCat.PoseidonEngine.Abstractions;
using LateCat.PoseidonEngine.Core;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace LateCat.Factory
{
    internal class PreviewerProvider : IPreviewerProvider
    {
        private readonly IPreviewerFactory _previewerFactory;

        public PreviewerProvider()
        {
            _previewerFactory = App.Services.GetRequiredService<IPreviewerFactory>();
        }

        public IPreviewer GetPreviewer(IWallpaperMetadata metadata)
        {
            switch (metadata.WallpaperInfo.Type)
            {
                case WallpaperType.Web:
                case WallpaperType.WebAudio:
                case WallpaperType.Url:
                    {
                        return _previewerFactory.Get<WebPreviewer>(metadata);
                    }
                case WallpaperType.Video:
                    {
                        return _previewerFactory.Get<VideoPreviewer>(metadata);
                    }
                case WallpaperType.Gif:
                case WallpaperType.Picture:
                    {
                        return _previewerFactory.Get<ImagePreviewer>(metadata);
                    }
                default:
                    {
                        throw new NotImplementedException();
                    }
            }
        }
    }
}
