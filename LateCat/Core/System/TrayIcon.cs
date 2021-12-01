using LateCat.Models;
using LateCat.PoseidonEngine.Abstractions;
using LateCat.PoseidonEngine.Core;
using LateCat.ViewModels;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;


namespace LateCat.Core
{
    internal class TrayIcon : ITrayIcon
    {
        private readonly NotifyIcon _notifyIcon = new();
        private ToolStripMenuItem _pauseWallpaperBtn;
        private ToolStripMenuItem _customWallpaperBtn;
        private static readonly Random _random = new();
        private bool _disposed;

        private readonly ISettingsService _settings;
        private readonly IPlayback _playback;
        private readonly IDesktopCore _desktopCore;
        private readonly WallpaperListViewModel _libraryVm;
        private readonly SettingsViewModel _settingsVm;
        private readonly MainWindow _mainWnd;

        public TrayIcon(
            ISettingsService settingsService,
            IPlayback playback,
            IDesktopCore desktopCore,
            WallpaperListViewModel wallpaperLibraryViewModel,
            SettingsViewModel settingsView,
            MainWindow mainWindow)
        {
            _settings = settingsService;
            _playback = playback;
            _desktopCore = desktopCore;
            _libraryVm = wallpaperLibraryViewModel;
            _settingsVm = settingsView;
            _mainWnd = mainWindow;

            var tt = new System.Windows.Controls.ToolTip
            {
                IsOpen = false
            };

            _notifyIcon.DoubleClick += (s, args) => Program.ShowMainWindow();

            _notifyIcon.Icon = Properties.Icons.favicon;
            _notifyIcon.Text = Properties.Resources.TitleAppName;

            CreateContextMenu();
            _notifyIcon.Visible = true;

            _desktopCore.WallpaperChanged += SetupDesktop_WallpaperChanged;
            _playback.PlaybackStateChanged += Playback_PlaybackStateChanged;
        }

        private void CreateContextMenu()
        {
            _notifyIcon.ContextMenuStrip = new ContextMenuStrip
            {
                ForeColor = Color.White,
                Padding = new Padding(0),
                Margin = new Padding(0),
            };

            _notifyIcon.ContextMenuStrip.Opening += ContextMenuStrip_Opening;

            _notifyIcon.ContextMenuStrip.Renderer = new Helpers.CustomContextMenu.RendererDark();

            _notifyIcon.ContextMenuStrip.Items.Add(Properties.Resources.TextOpenLateCat, Properties.Icons.home).Click += (s, e) => Program.ShowMainWindow();

            _notifyIcon.ContextMenuStrip.Items.Add(Properties.Resources.TextCloseWallpapers, null).Click += (s, e) => _desktopCore.CloseAllWallpapers(true);

            _pauseWallpaperBtn = new ToolStripMenuItem(Properties.Resources.TextPauseWallpapers, Properties.Icons.pause);
            _pauseWallpaperBtn.Click += (s, e) => ToggleWallpaperPlaybackState(s, e);
            _notifyIcon.ContextMenuStrip.Items.Add(_pauseWallpaperBtn);

            _notifyIcon.ContextMenuStrip.Items.Add(Properties.Resources.TextChangeWallpaper, null).Click += (s, e) => SetNextWallpaper();

            _customWallpaperBtn = new ToolStripMenuItem(Properties.Resources.TextCustomiseWallpaper, Properties.Icons.custom)
            {
                Enabled = false
            };
            _customWallpaperBtn.Click += CustomiseWallpaper;
            _notifyIcon.ContextMenuStrip.Items.Add(_customWallpaperBtn);

            _notifyIcon.ContextMenuStrip.Items.Add(Properties.Resources.TextExit, Properties.Icons.exit).Click += (s, e) => Program.ExitApplication();
        }

        private void ContextMenuStrip_Opening(object? sender, CancelEventArgs e)
        {
            var menuStrip = sender as ContextMenuStrip ?? throw new ArgumentNullException(nameof(sender));

            if (MonitorHelper.IsMultiMonitor())
            {
                var Monitor = MonitorHelper.GetMonitorFromPoint(Cursor.Position);

                var mousePos = Cursor.Position;

                mousePos.X += -1 * Monitor.Bounds.X;
                mousePos.Y += -1 * Monitor.Bounds.Y;

                bool isLeft = mousePos.X < Monitor.Bounds.Width * .5;
                bool isTop = mousePos.Y < Monitor.Bounds.Height * .5;

                if (isLeft && isTop)
                {
                    menuStrip.Show(Cursor.Position, ToolStripDropDownDirection.Default);
                }
                if (isLeft && !isTop)
                {
                    menuStrip.Show(Cursor.Position, ToolStripDropDownDirection.AboveRight);
                }
                else if (!isLeft && isTop)
                {
                    menuStrip.Show(Cursor.Position, ToolStripDropDownDirection.BelowLeft);
                }
                else if (!isLeft && !isTop)
                {
                    menuStrip.Show(Cursor.Position, ToolStripDropDownDirection.AboveLeft);
                }
            }
            else
            {
                menuStrip.Show(Cursor.Position, ToolStripDropDownDirection.AboveLeft);
            }
        }

        public void ShowBalloonNotification(int timeout, string title, string msg)
        {
            _notifyIcon.ShowBalloonTip(timeout, title, msg, ToolTipIcon.None);
        }

        private void CustomiseWallpaper(object? sender, EventArgs e)
        {
            //var items = SetupDesktop.Wallpapers.FindAll(x => x.GetWallpaperData().PropertyPath != null);
            //if (items.Count == 0)
            //{
            //    //not possible, menu should be disabled.
            //    //nothing..
            //}
            //else if (items.Count == 1)
            //{
            //    //quick wallpaper customise tray widget.
            //    var settingsWidget = new Cef.PropertiesTrayWidget(items[0].GetWallpaperData());
            //    settingsWidget.Show();
            //}
            //else if (items.Count > 1)
            //{
            //    switch (Program.SettingsVM.Settings.WallpaperArrangement)
            //    {
            //        case WallpaperArrangement.Per:
            //            //multiple different wallpapers.. open control panel.
            //            App.AppWindow?.ShowControlPanelDialog();
            //            break;
            //        case WallpaperArrangement.Span:
            //        case WallpaperArrangement.Duplicate:
            //            var settingsWidget = new Cef.PropertiesTrayWidget(items[0].GetWallpaperData());
            //            settingsWidget.Show();
            //            break;
            //    }
            //}
        }

        private void SetNextWallpaper()
        {
            if (_libraryVm.Items.Count == 0)
            {
                return;
            }

            switch (_settings.Settings.WallpaperArrangement)
            {
                case WallpaperArrangement.Per:
                    {
                        if (_desktopCore.Wallpapers.Count == 0)
                        {
                            foreach (var Monitor in MonitorHelper.GetMonitor())
                            {
                                _desktopCore.SetWallpaper(_libraryVm.Items[_random.Next(_libraryVm.Items.Count)], Monitor);
                            }
                        }
                        else
                        {
                            var wallpapers = _desktopCore.Wallpapers.ToList();
                            foreach (var wp in wallpapers)
                            {
                                var index = _libraryVm.Items.IndexOf((Wallpaper)wp.Metadata);
                                if (index != -1)
                                {
                                    index = (index + 1) != _libraryVm.Items.Count ? (index + 1) : 0;
                                    _desktopCore.SetWallpaper(_libraryVm.Items[index], wp.Monitor);
                                }
                            }
                        }
                    }
                    break;
                case WallpaperArrangement.Span:
                case WallpaperArrangement.Duplicate:
                    {
                        var wallpaper = _desktopCore.Wallpapers.Count != 0 ?
                             _desktopCore.Wallpapers[0].Metadata : _libraryVm.Items[_random.Next(_libraryVm.Items.Count)];
                        var index = _libraryVm.Items.IndexOf(wallpaper);
                        if (index != -1)
                        {
                            index = (index + 1) != _libraryVm.Items.Count ? (index + 1) : 0;
                            _desktopCore.SetWallpaper(_libraryVm.Items[index], MonitorHelper.GetPrimaryMonitor());
                        }
                    }
                    break;
            }
        }

        private void Playback_PlaybackStateChanged(object? sender, PlaybackState e)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new ThreadStart(delegate
            {
                _pauseWallpaperBtn.Checked = e == PlaybackState.Paused;
                _notifyIcon.Icon = (e == PlaybackState.Paused) ? Properties.Icons.favicon : Properties.Icons.favicon;
            }));
        }

        private void ToggleWallpaperPlaybackState(object? sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem ?? throw new ArgumentNullException(nameof(sender));

            if (_playback.WallpaperPlaybackState == PlaybackState.Play)
            {
                item.Text = "播放壁纸";
                item.Image = Properties.Icons.play;
                _playback.WallpaperPlaybackState = PlaybackState.Paused;
            }
            else
            {
                item.Text = "暂停壁纸";
                item.Image = Properties.Icons.pause;
                _playback.WallpaperPlaybackState = PlaybackState.Play;
            }
        }

        private void SetupDesktop_WallpaperChanged(object? sender, EventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new ThreadStart(delegate
            {
                _customWallpaperBtn.Enabled = _desktopCore.Wallpapers.Any(x => x.Metadata.PropertyPath != null);
            }));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _playback.PlaybackStateChanged -= Playback_PlaybackStateChanged;
                    _notifyIcon.Visible = false;
                    _notifyIcon?.Icon.Dispose();
                    _notifyIcon?.Dispose();
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