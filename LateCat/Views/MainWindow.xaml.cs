using LateCat.Helpers;
using LateCat.PoseidonEngine.Abstractions;
using LateCat.PoseidonEngine.Core;
using LateCat.ViewModels;
using LateCat.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace LateCat
{
    public partial class MainWindow : Window
    {
        private MonitorLayoutView _layoutWnd;
        private SettingsView _settingsView;
        private WallpaperListViewModel _wallpaperListVm;
        private ISettingsService _settings;

        public static bool IsExit { get; set; } = false;

        public MainWindow()
        {
            InitializeComponent();            

            App.Services.GetRequiredService<IDesktopCore>().WallpaperChanged += SetupDesktop_WallpaperChanged;

            DataContext = this;            
        }

        private void SetupDesktop_WallpaperChanged(object? sender, EventArgs e)
        {
            _ = Dispatcher.BeginInvoke(new Action(() =>
            {
                var settings = App.Services.GetRequiredService<ISettingsService>();
                if (!settings.Settings.ControlPanelOpened &&
                    WindowState != WindowState.Minimized &&
                    Visibility == Visibility.Visible)
                {
                    settings.Settings.ControlPanelOpened = true;
                    settings.SaveSettings();
                }
                if (IsVisible && (_layoutWnd == null || _layoutWnd.Visibility != Visibility.Visible))
                {
                    Activate();
                }
            }));
        }

        public void ShowControlPanelDialog()
        {
            if (_layoutWnd is null)
            {
                _layoutWnd = new MonitorLayoutView()
                {
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Width = Width / 1.5,
                    Height = Height / 1.5
                };

                _layoutWnd.Closed += LayoutWindow_Closed;
                _layoutWnd.Show();
            }
            else
            {
                _layoutWnd.Activate();
            }
        }

        private void LayoutWindow_Closed(object? sender, EventArgs e)
        {
            _layoutWnd = null;
            Activate();
        }

        private void BtnStatus_Click(object sender, RoutedEventArgs e)
        {
            ShowControlPanelDialog();
        }

        private void ColorZone_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _wallpaperListVm = App.Services.GetRequiredService<WallpaperListViewModel>();
            _settings = App.Services.GetRequiredService<ISettingsService>();

            var wallpaperLv = (WallpaperListView)Frame.Content;

            wallpaperLv.ContextMenuClick += WallpaperListView_ContextMenuClick;
            wallpaperLv.FileDroppedEvent += WallpaperListView_FileDroppedEvent;            
        }

        private void WallpaperListView_ContextMenuClick(object? sender, object e)
        {
            throw new NotImplementedException();
        }

        private void WallpaperListView_FileDroppedEvent(object? sender, DragEventArgs e)
        {
            CrossThreadAccessor.RunAsync(() =>
            {
                var items = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (items.Length == 1)
                {
                    var type = FileFilter.GetFileType(items[0]);

                    switch (type)
                    {
                        case WallpaperType.Web:
                        case WallpaperType.WebAudio:
                        case WallpaperType.Url:
                        case WallpaperType.Video:
                        case WallpaperType.Gif:
                        case WallpaperType.VideoStream:
                        case WallpaperType.Picture:
                            {
                                _wallpaperListVm.AddWallpaper(
                                    items[0],
                                    type,
                                    WallpaperProcessStatus.Processing,
                                    _settings.Settings.SelectedMonitor);
                            }
                            break;
                        default:
                            MessageBox.Show("没有识别到合适的壁纸");
                            break;
                    }
                }
                else if (items.Length > 1)
                {

                }
            });
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            GC.Collect();
        }

        private void WndMain_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!IsExit)
            {
                e.Cancel = true;

                Hide();
                GC.Collect();
            }
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            if (_settingsView is null)
            {
                _settingsView = new SettingsView()
                {
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                _settingsView.Closed += SettingsView_Closed;
                _settingsView.Show();
            }
            else
            {
                _settingsView.Activate();
            }
        }

        private void SettingsView_Closed(object? sender, EventArgs e)
        {
            _settingsView = null;
            Activate();
        }

        private RelayCommand _previousCommand;
        public RelayCommand PreviousCommand
        {
            get
            {
                if (_previousCommand is null)
                {
                    _previousCommand = new RelayCommand(
                        parm => _wallpaperListVm.PreviousWallpaper());
                }

                return _previousCommand;
            }
            private set
            {
                _previousCommand = value;
            }
        }

        private RelayCommand _nextCommand;
        public RelayCommand NextCommand
        {
            get
            {
                if (_nextCommand is null)
                {
                    _nextCommand = new RelayCommand(
                        parm => _wallpaperListVm.NextWallpaper());
                }

                return _nextCommand;
            }
            private set
            {
                _nextCommand = value;
            }
        }

        private RelayCommand _switchCommand;
        public RelayCommand SwitchCommand
        {
            get
            {
                if (_switchCommand is null)
                {
                    _switchCommand = new RelayCommand(
                        parm => _wallpaperListVm.SwitchWallpaper());
                }

                return _switchCommand;
            }
            private set
            {
                _switchCommand = value;
            }
        }

    }
}
