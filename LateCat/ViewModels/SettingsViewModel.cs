using LateCat.Helpers;
using LateCat.PoseidonEngine.Abstractions;
using LateCat.PoseidonEngine.Core;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
namespace LateCat.ViewModels
{
    public class SettingsViewModel : ObservableObject
    {
        private readonly ISettingsService _settings;
        private readonly IDesktopCore _desktopCore;
        private readonly ITaskbarOperator _tbOperator;

        public SettingsViewModel(ISettingsService settings, IDesktopCore desktopCore, ITaskbarOperator tbOperator)
        {
            _settings = settings;
            _desktopCore = desktopCore;
            _tbOperator = tbOperator;

            IsStartup = WindowsStartup.CheckStartupRegistry() == 1 || WindowsStartup.CheckStartupRegistry() == -1;

            settings.Settings.SelectedMonitor = MonitorHelper.GetMonitor(settings.Settings.SelectedMonitor.DeviceId, settings.Settings.SelectedMonitor.DeviceName,
                        settings.Settings.SelectedMonitor.Bounds, settings.Settings.SelectedMonitor.WorkingArea, MonitorIdentificationMode.DeviceId) ?? MonitorHelper.GetPrimaryMonitor();

            IsStartup = settings.Settings.Startup;
            SelectedTileSizeIndex = settings.Settings.TileSize;
            SelectedAppFullScreenIndex = (int)settings.Settings.OtherAppFullScreen;
            SelectedAppFocusIndex = (int)settings.Settings.OtherAppFocus;
            SelectedBatteryPowerIndex = (int)settings.Settings.InBatteryMode;
            SelectedRemoteDestopPowerIndex = (int)settings.Settings.InRemoteDesktop;
            SelectedPowerSaveModeIndex = (int)settings.Settings.PowerSaveMode;
            SelectedDisplayPauseRuleIndex = (int)settings.Settings.MonitorPauseStrategy;
            SelectedPauseAlgorithmIndex = (int)settings.Settings.ProcessMonitorAlgorithm;
            SelectedVideoPlayerIndex = (int)settings.Settings.VideoPlayer;
            VideoPlayerHWDecode = settings.Settings.VideoPlayerHwAccel;
            SelectedGifPlayerIndex = (int)settings.Settings.GifPlayer;
            SelectedWallpaperStreamQualityIndex = (int)settings.Settings.StreamQuality;
            SelectedWallpaperInputMode = (int)settings.Settings.InputForward;
            MouseMoveOnDesktop = settings.Settings.MouseInputMoveAlways;
            DetectStreamWallpaper = settings.Settings.AutoDetectOnlineStreams;
            WallpaperDirectory = settings.Settings.WallpaperDir;
            MoveExistingWallpaperNewDir = settings.Settings.WallpaperDirMoveExistingWallpaperNewDir;
            GlobalWallpaperVolume = settings.Settings.AudioVolumeGlobal;
            IsAudioOnlyOnDesktop = settings.Settings.AudioOnlyOnDesktop;
            SelectedWallpaperScalingIndex = (int)settings.Settings.WallpaperScaling;
            CefDiskCache = settings.Settings.CefDiskCache;
            SelectedTaskbarThemeIndex = (int)settings.Settings.SystemTaskbarTheme;
            IsDesktopAutoWallpaper = settings.Settings.DesktopAutoWallpaper;
            SelectedWebBrowserIndex = (int)settings.Settings.WebBrowser;
        }

        public void UpdateConfigFile()
        {
            _settings.SaveSettings();
        }

        #region general

        private bool _isStartup;
        public bool IsStartup
        {
            get
            {
                return _isStartup;
            }
            set
            {
                _isStartup = value;
                _settings.Settings.Startup = _isStartup;
                OnPropertyChanged();

                WindowsStartup.SetStartupRegistry(_isStartup);
                UpdateConfigFile();
            }
        }

        private int _selectedTileSizeIndex;
        public int SelectedTileSizeIndex
        {
            get
            {
                return _selectedTileSizeIndex;
            }
            set
            {
                _selectedTileSizeIndex = value;
                OnPropertyChanged();

                if (_settings.Settings.TileSize != _selectedTileSizeIndex)
                {
                    _settings.Settings.TileSize = _selectedTileSizeIndex;
                    UpdateConfigFile();
                }
            }
        }

        private string _wallpaperDirectory;
        public string WallpaperDirectory
        {
            get { return _wallpaperDirectory; }
            set
            {
                _wallpaperDirectory = value;
                OnPropertyChanged();
            }
        }

        private RelayCommand _wallpaperDirectoryChangeCommand;
        public RelayCommand WallpaperDirectoryChangeCommand
        {
            get
            {
                if (_wallpaperDirectoryChangeCommand == null)
                {
                    _wallpaperDirectoryChangeCommand = new RelayCommand(
                        param => WallpaperDirectoryChange(), param => !WallpapeDirectoryChanging);
                }
                return _wallpaperDirectoryChangeCommand;
            }
        }

        private bool _wallpapeDirectoryChanging;
        public bool WallpapeDirectoryChanging
        {
            get { return _wallpapeDirectoryChanging; }
            set
            {
                _wallpapeDirectoryChanging = value;
                OnPropertyChanged();
            }
        }

        private bool _moveExistingWallpaperNewDir;
        public bool MoveExistingWallpaperNewDir
        {
            get { return _moveExistingWallpaperNewDir; }
            set
            {
                _moveExistingWallpaperNewDir = value;
                OnPropertyChanged();

                if (_settings.Settings.WallpaperDirMoveExistingWallpaperNewDir != _moveExistingWallpaperNewDir)
                {
                    _settings.Settings.WallpaperDirMoveExistingWallpaperNewDir = _moveExistingWallpaperNewDir;
                    UpdateConfigFile();
                }
            }
        }

        private RelayCommand _openWallpaperDirectory;
        public RelayCommand OpenWallpaperDirectory
        {
            get
            {
                if (_openWallpaperDirectory == null)
                {
                    _openWallpaperDirectory = new RelayCommand(
                            param => FileOperations.OpenFolder(_settings.Settings.WallpaperDir)
                        );
                }
                return _openWallpaperDirectory;
            }
        }

        #endregion general

        #region performance

        private int _selectedAppFullScreenIndex;
        public int SelectedAppFullScreenIndex
        {
            get
            {
                return _selectedAppFullScreenIndex;
            }
            set
            {
                _selectedAppFullScreenIndex = value;
                OnPropertyChanged();

                if (_settings.Settings.OtherAppFullScreen != (PerformanceStrategy)_selectedAppFullScreenIndex)
                {
                    _settings.Settings.OtherAppFullScreen = (PerformanceStrategy)_selectedAppFullScreenIndex;
                    UpdateConfigFile();
                }
            }
        }

        private int _selectedAppFocusIndex;
        public int SelectedAppFocusIndex
        {
            get
            {
                return _selectedAppFocusIndex;
            }
            set
            {
                _selectedAppFocusIndex = value;
                OnPropertyChanged();

                if (_settings.Settings.OtherAppFocus != (PerformanceStrategy)_selectedAppFocusIndex)
                {
                    _settings.Settings.OtherAppFocus = (PerformanceStrategy)_selectedAppFocusIndex;
                    UpdateConfigFile();
                }
            }
        }

        private int _selectedBatteryPowerIndex;
        public int SelectedBatteryPowerIndex
        {
            get
            {
                return _selectedBatteryPowerIndex;
            }
            set
            {
                _selectedBatteryPowerIndex = value;
                OnPropertyChanged();

                if (_settings.Settings.InBatteryMode != (PerformanceStrategy)_selectedBatteryPowerIndex)
                {
                    _settings.Settings.InBatteryMode = (PerformanceStrategy)_selectedBatteryPowerIndex;
                    UpdateConfigFile();
                }
            }
        }

        private int _selectedPowerSaveModeIndex;
        public int SelectedPowerSaveModeIndex
        {
            get
            {
                return _selectedPowerSaveModeIndex;
            }
            set
            {
                _selectedPowerSaveModeIndex = value;
                OnPropertyChanged();

                if (_settings.Settings.PowerSaveMode != (PerformanceStrategy)_selectedPowerSaveModeIndex)
                {
                    _settings.Settings.PowerSaveMode = (PerformanceStrategy)_selectedPowerSaveModeIndex;
                    UpdateConfigFile();
                }
            }
        }

        private int _selectedRemoteDestopPowerIndex;
        public int SelectedRemoteDestopPowerIndex
        {
            get
            {
                return _selectedRemoteDestopPowerIndex;
            }
            set
            {
                _selectedRemoteDestopPowerIndex = value;
                OnPropertyChanged();

                if (_settings.Settings.InRemoteDesktop != (PerformanceStrategy)_selectedRemoteDestopPowerIndex)
                {
                    _settings.Settings.InRemoteDesktop = (PerformanceStrategy)_selectedRemoteDestopPowerIndex;
                    UpdateConfigFile();
                }
            }
        }

        private int _selectedDisplayPauseRuleIndex;
        public int SelectedDisplayPauseRuleIndex
        {
            get
            {
                return _selectedDisplayPauseRuleIndex;
            }
            set
            {
                _selectedDisplayPauseRuleIndex = value;
                OnPropertyChanged();

                if (_settings.Settings.MonitorPauseStrategy != (MonitorPauseStrategy)_selectedDisplayPauseRuleIndex)
                {
                    _settings.Settings.MonitorPauseStrategy = (MonitorPauseStrategy)_selectedDisplayPauseRuleIndex;
                    UpdateConfigFile();
                }
            }
        }

        private int _selectedPauseAlgorithmIndex;
        public int SelectedPauseAlgorithmIndex
        {
            get
            {
                return _selectedPauseAlgorithmIndex;
            }
            set
            {
                _selectedPauseAlgorithmIndex = value;
                OnPropertyChanged();

                if (_settings.Settings.ProcessMonitorAlgorithm != (ProcessMonitorAlgorithm)_selectedPauseAlgorithmIndex)
                {
                    _settings.Settings.ProcessMonitorAlgorithm = (ProcessMonitorAlgorithm)_selectedPauseAlgorithmIndex;
                    UpdateConfigFile();
                }
            }
        }

        #endregion performance

        #region wallpaper

        private int _selectedWallpaperScalingIndex;
        public int SelectedWallpaperScalingIndex
        {
            get { return _selectedWallpaperScalingIndex; }
            set
            {
                _selectedWallpaperScalingIndex = value;
                OnPropertyChanged();

                if (_settings.Settings.WallpaperScaling != (WallpaperScaler)_selectedWallpaperScalingIndex)
                {
                    _settings.Settings.WallpaperScaling = (WallpaperScaler)_selectedWallpaperScalingIndex;
                    UpdateConfigFile();

                    WallpaperRestart(WallpaperType.VideoStream);
                    WallpaperRestart(WallpaperType.Video);
                    WallpaperRestart(WallpaperType.Gif);
                    WallpaperRestart(WallpaperType.Picture);
                }
            }
        }

        private int _selectedWallpaperInputMode;
        public int SelectedWallpaperInputMode
        {
            get { return _selectedWallpaperInputMode; }
            set
            {
                _selectedWallpaperInputMode = value;
                OnPropertyChanged();

                if (_settings.Settings.InputForward != (InputForwardMode)_selectedWallpaperInputMode)
                {
                    _settings.Settings.InputForward = (InputForwardMode)_selectedWallpaperInputMode;
                    UpdateConfigFile();
                }

                if (_settings.Settings.InputForward == InputForwardMode.MouseKeyboard)
                {
                    DesktopUtilities.SetDesktopIconVisibility(false);
                }
                else
                {
                    DesktopUtilities.SetDesktopIconVisibility(DesktopUtilities.DesktopIconVisibilityDefault);
                }
            }
        }

        private int _selectedVideoPlayerIndex;
        public int SelectedVideoPlayerIndex
        {
            get
            {
                return _selectedVideoPlayerIndex;
            }
            set
            {
                _selectedVideoPlayerIndex = CheckVideoPluginExists((MediaPlayerType)value) ? value : (int)MediaPlayerType.MPV;
                OnPropertyChanged();

                if (_settings.Settings.VideoPlayer != (MediaPlayerType)_selectedVideoPlayerIndex)
                {
                    _settings.Settings.VideoPlayer = (MediaPlayerType)_selectedVideoPlayerIndex;
                    UpdateConfigFile();
                    //VideoPlayerSwitch((LateCatMediaPlayer)_selectedVideoPlayerIndex);
                    WallpaperRestart(WallpaperType.Video);
                }
            }
        }

        private bool _videoPlayerHWDecode;
        public bool VideoPlayerHWDecode
        {
            get { return _videoPlayerHWDecode; }
            set
            {
                _videoPlayerHWDecode = value;
                OnPropertyChanged();
                if (_settings.Settings.VideoPlayerHwAccel != _videoPlayerHWDecode)
                {
                    _settings.Settings.VideoPlayerHwAccel = _videoPlayerHWDecode;
                    UpdateConfigFile();
                    WallpaperRestart(WallpaperType.Video);
                    WallpaperRestart(WallpaperType.VideoStream);
                    WallpaperRestart(WallpaperType.Gif);
                }
            }
        }

        private int _selectedGifPlayerIndex;
        public int SelectedGifPlayerIndex
        {
            get
            {
                return _selectedGifPlayerIndex;
            }
            set
            {
                _selectedGifPlayerIndex = CheckGifPluginExists((GifPlayer)value) ? value : (int)GifPlayer.MPV;
                OnPropertyChanged();
                if (_settings.Settings.GifPlayer != (GifPlayer)_selectedGifPlayerIndex)
                {
                    _settings.Settings.GifPlayer = (GifPlayer)_selectedGifPlayerIndex;
                    UpdateConfigFile();
                    WallpaperRestart(WallpaperType.Gif);
                    WallpaperRestart(WallpaperType.Picture);
                }
            }
        }

        private int _selectedWallpaperStreamQualityIndex;
        public int SelectedWallpaperStreamQualityIndex
        {
            get { return _selectedWallpaperStreamQualityIndex; }
            set
            {
                _selectedWallpaperStreamQualityIndex = value;
                OnPropertyChanged();
                if (_settings.Settings.StreamQuality != (StreamQualitySuggestion)_selectedWallpaperStreamQualityIndex)
                {
                    _settings.Settings.StreamQuality = (StreamQualitySuggestion)_selectedWallpaperStreamQualityIndex;
                    UpdateConfigFile();

                    WallpaperRestart(WallpaperType.VideoStream);
                }
            }
        }

        private int _selectedWebBrowserIndex;
        public int SelectedWebBrowserIndex
        {
            get
            {
                return _selectedWebBrowserIndex;
            }
            set
            {
                _selectedWebBrowserIndex = value;
                OnPropertyChanged();

                if (_settings.Settings.WebBrowser != (PoseidonEngine.Core.WebBrowser)_selectedWebBrowserIndex)
                {
                    _settings.Settings.WebBrowser = (PoseidonEngine.Core.WebBrowser)_selectedWebBrowserIndex;
                    UpdateConfigFile();

                    WallpaperRestart(WallpaperType.Web);
                    WallpaperRestart(WallpaperType.WebAudio);
                    WallpaperRestart(WallpaperType.Url);
                }
            }
        }

        private bool _mouseMoveOnDesktop;
        public bool MouseMoveOnDesktop
        {
            get { return _mouseMoveOnDesktop; }
            set
            {
                _mouseMoveOnDesktop = value;
                OnPropertyChanged();

                if (_settings.Settings.MouseInputMoveAlways != _mouseMoveOnDesktop)
                {
                    _settings.Settings.MouseInputMoveAlways = _mouseMoveOnDesktop;
                    UpdateConfigFile();
                }
            }
        }

        private bool _cefDiskCache;
        public bool CefDiskCache
        {
            get { return _cefDiskCache; }
            set
            {
                _cefDiskCache = value;
                if (_settings.Settings.CefDiskCache != _cefDiskCache)
                {
                    _settings.Settings.CefDiskCache = _cefDiskCache;
                    UpdateConfigFile();
                }
                OnPropertyChanged();
            }
        }

        private bool _detectStreamWallpaper;
        public bool DetectStreamWallpaper
        {
            get { return _detectStreamWallpaper; }
            set
            {
                _detectStreamWallpaper = value;
                if (_settings.Settings.AutoDetectOnlineStreams != _detectStreamWallpaper)
                {
                    _settings.Settings.AutoDetectOnlineStreams = _detectStreamWallpaper;
                    UpdateConfigFile();
                }
                OnPropertyChanged();
            }
        }

        #endregion wallpaper

        #region audio

        private int _globalWallpaperVolume;
        public int GlobalWallpaperVolume
        {
            get { return _globalWallpaperVolume; }
            set
            {
                _globalWallpaperVolume = value;
                if (_settings.Settings.AudioVolumeGlobal != _globalWallpaperVolume)
                {
                    _settings.Settings.AudioVolumeGlobal = _globalWallpaperVolume;
                    UpdateConfigFile();
                }
                OnPropertyChanged();
            }
        }

        private bool _isAudioOnlyOnDesktop;
        public bool IsAudioOnlyOnDesktop
        {
            get
            {
                return _isAudioOnlyOnDesktop;
            }
            set
            {
                _isAudioOnlyOnDesktop = value;
                if (_settings.Settings.AudioOnlyOnDesktop != _isAudioOnlyOnDesktop)
                {
                    _settings.Settings.AudioOnlyOnDesktop = _isAudioOnlyOnDesktop;
                    UpdateConfigFile();
                }
                OnPropertyChanged();
            }
        }

        #endregion //audio

        #region system       

        private bool _isDesktopAutoWallpaper;
        public bool IsDesktopAutoWallpaper
        {
            get
            {
                return _isDesktopAutoWallpaper;
            }
            set
            {
                _isDesktopAutoWallpaper = value;
                if (_settings.Settings.DesktopAutoWallpaper != _isDesktopAutoWallpaper)
                {
                    _settings.Settings.DesktopAutoWallpaper = _isDesktopAutoWallpaper;
                    UpdateConfigFile();
                }
                OnPropertyChanged();
            }
        }

        private bool ttbInitialized = false;
        private int _selectedTaskbarThemeIndex;
        public int SelectedTaskbarThemeIndex
        {
            get
            {
                return _selectedTaskbarThemeIndex;
            }
            set
            {
                _selectedTaskbarThemeIndex = value;
                if (!ttbInitialized)
                {
                    if ((TaskbarTheme)_selectedTaskbarThemeIndex != TaskbarTheme.None)
                    {
                        string pgm = null;
                        if ((pgm = _tbOperator.CheckIncompatiblePrograms()) == null)
                        {
                            _tbOperator.Start((TaskbarTheme)_selectedTaskbarThemeIndex);
                            ttbInitialized = true;
                        }
                        else
                        {
                            _selectedTaskbarThemeIndex = (int)TaskbarTheme.None;
                            _ = Task.Run(() =>
                                    System.Windows.MessageBox.Show("发现不兼容的应用程序\n\n" + pgm,
                                        Properties.Resources.TitleAppName, MessageBoxButton.OK, MessageBoxImage.Information));
                        }
                    }
                }
                else
                {
                    _tbOperator.Start((TaskbarTheme)_selectedTaskbarThemeIndex);
                }
                //save the data..
                if (_settings.Settings.SystemTaskbarTheme != (TaskbarTheme)_selectedTaskbarThemeIndex)
                {
                    _settings.Settings.SystemTaskbarTheme = (TaskbarTheme)_selectedTaskbarThemeIndex;
                    UpdateConfigFile();
                }
                OnPropertyChanged();
            }
        }

        #endregion //system


        #region helper fns

        private static bool CheckVideoPluginExists(MediaPlayerType mp)
        {
            return mp switch
            {
                MediaPlayerType.Wmf => true,
                MediaPlayerType.LibVLC => false,
                MediaPlayerType.LibMPV => false,
                MediaPlayerType.LibVLCExt => File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", "libVLCPlayer", "libVLCPlayer.exe")),
                MediaPlayerType.LibMPVExt => File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", "libMPVPlayer", "libMPVPlayer.exe")),
                MediaPlayerType.MPV => File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", "mpv", "mpv.exe")),
                MediaPlayerType.VLC => File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", "vlc", "vlc.exe")),
                _ => false,
            };
        }

        private static bool CheckGifPluginExists(GifPlayer gp)
        {
            return gp switch
            {
                GifPlayer.LibMPVExt => File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", "libMPVPlayer", "libMPVPlayer.exe")),
                GifPlayer.MPV => File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", "mpv", "mpv.exe")),
                _ => false,
            };
        }

        private void WallpaperRestart(WallpaperType type)
        {

            var originalWallpapers = _desktopCore.Wallpapers.Where(x => x.WallpaperType == type).ToList();
            if (originalWallpapers.Count > 0)
            {
                _desktopCore.CloseWallpaper(type, true);
                foreach (var item in originalWallpapers)
                {
                    _desktopCore.SetWallpaper(item.Metadata, item.Monitor);
                    if (_settings.Settings.WallpaperArrangement == WallpaperArrangement.Span
                        || _settings.Settings.WallpaperArrangement == WallpaperArrangement.Duplicate)
                    {
                        break;
                    }
                }
            }
        }

        public event EventHandler<string> WallpaperDirChange;
        private async void WallpaperDirectoryChange()
        {
            bool isDestEmptyDir = false;
            var folderBrowserDialog = new FolderBrowserDialog
            {
                SelectedPath = Program.WallpaperDir
            };
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                if (string.Equals(folderBrowserDialog.SelectedPath, _settings.Settings.WallpaperDir, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                try
                {
                    var parentDir = Directory.GetParent(folderBrowserDialog.SelectedPath).ToString();
                    if (parentDir != null)
                    {
                        if (Directory.Exists(Path.Combine(parentDir, "wallpapers")) &&
                            Directory.Exists(Path.Combine(parentDir, ".late", "data")))
                        {
                            var result = System.Windows.MessageBox.Show("您的意思是选择" + parentDir +
                                "\n吗？Late Cat 需要 '.late' 和 'wallpapers' 文件夹!",
                                Properties.Resources.TitlePleaseWait,
                                MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                            switch (result)
                            {
                                case MessageBoxResult.Yes:
                                    folderBrowserDialog.SelectedPath = parentDir;
                                    break;
                                case MessageBoxResult.No:
                                    break;
                            }
                        }
                    }

                    WallpapeDirectoryChanging = true;
                    RelayCommand.RaiseCanExecuteChanged();

                    Directory.CreateDirectory(folderBrowserDialog.SelectedPath);

                    if (_settings.Settings.WallpaperDirMoveExistingWallpaperNewDir)
                    {
                        await Task.Run(() =>
                        {
                            FileOperations.DirectoryCopy(Program.WallpaperDir, folderBrowserDialog.SelectedPath, true);
                        });
                    }
                    else
                    {
                        isDestEmptyDir = true;
                    }
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show("Failed to write to new directory:\n" + e.Message, Properties.Resources.TextError);
                    return;
                }
                finally
                {
                    WallpapeDirectoryChanging = false;
                    RelayCommand.RaiseCanExecuteChanged();
                }

                _desktopCore.CloseAllWallpapers(true);

                var previousDirectory = _settings.Settings.WallpaperDir;
                _settings.Settings.WallpaperDir = folderBrowserDialog.SelectedPath;
                UpdateConfigFile();
                WallpaperDirectory = _settings.Settings.WallpaperDir;
                Program.WallpaperDir = _settings.Settings.WallpaperDir;
                WallpaperDirChange?.Invoke(null, folderBrowserDialog.SelectedPath);

                if (!isDestEmptyDir)
                {
                    var result1 = await FileOperations.DeleteDirectoryAsync(previousDirectory, 1000, 3000);
                    if (!result1)
                    {
                        System.Windows.MessageBox.Show("Failed to delete old wallpaper directory!\nTry deleting it manually.", Properties.Resources.TextError);
                    }
                }
                folderBrowserDialog.Dispose();
            }
        }

        #endregion //helper fns
    }
}
