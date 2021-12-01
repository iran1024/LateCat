using LateCat.PoseidonEngine.Abstractions;
using LateCat.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace LateCat.Core
{
    class WallpaperMonitor : IWallpaperMonitor
    {
        private readonly FileSystemWatcher _watcher;

        private readonly WallpaperListViewModel _wallpaperListVm;

        private bool _disposed;

        public WallpaperMonitor()
        {
            _watcher = new FileSystemWatcher(Program.WallpaperDir)
            {
                NotifyFilter = NotifyFilters.DirectoryName,
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };

            _wallpaperListVm = App.Services.GetRequiredService<WallpaperListViewModel>();
        }

        public void Start()
        {
            _watcher.Created += Watcher_Created;
            _watcher.Deleted += Watcher_Deleted;
        }


        public void Stop()
        {
            _watcher.Created -= Watcher_Created;
            _watcher.Deleted -= Watcher_Deleted;

            _watcher.EnableRaisingEvents = false;

            _watcher.Dispose();
        }

        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            var metadata = _wallpaperListVm.GetWallpaper(e.FullPath);

            if (metadata is not null)
            {
                _wallpaperListVm.WallpaperDelete(metadata);
            }
        }

        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            if (File.Exists(Path.Combine(e.FullPath, "WallpaperInfo.json")))
            {
                _wallpaperListVm.AddWallpaper(e.FullPath);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Stop();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
