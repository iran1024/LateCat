using LateCat.Models;
using LateCat.PoseidonEngine.Abstractions;
using LateCat.PoseidonEngine.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace LateCat.ViewModels
{
    public class MonitorLayoutViewModel : ObservableObject
    {
        private readonly ISettingsService _settings;
        private readonly IDesktopCore _desktopCore;
        private readonly WallpaperListViewModel _wallpaperListVm;

        public MonitorLayoutViewModel(ISettingsService settings, IDesktopCore desktopCore, WallpaperListViewModel wallpaperListVm)
        {
            _settings = settings;
            _desktopCore = desktopCore;
            _wallpaperListVm = wallpaperListVm;

            SelectedWallpaperLayout = (int)settings.Settings.WallpaperArrangement;
            MonitorItems = new ObservableCollection<ScreenLayoutModel>();
            UpdateLayout();

            desktopCore.WallpaperChanged += SetupDesktop_WallpaperChanged;
        }

        private void SetupDesktop_WallpaperChanged(object? sender, EventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
                new System.Threading.ThreadStart(delegate
                {
                    UpdateLayout();
                }));
        }

        private ObservableCollection<ScreenLayoutModel> _monitorItems;
        public ObservableCollection<ScreenLayoutModel> MonitorItems
        {
            get { return _monitorItems; }
            set
            {
                if (value != _monitorItems)
                {
                    _monitorItems = value;
                    OnPropertyChanged();
                }
            }
        }

        private ScreenLayoutModel _selectedItem;
        public ScreenLayoutModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value != null)
                {
                    _selectedItem = value;
                    OnPropertyChanged();
                    CanCloseWallpaper();
                    CanCustomiseWallpaper();
                    if (!MonitorHelper.Compare(value.Monitor, _settings.Settings.SelectedMonitor, MonitorIdentificationMode.DeviceId))
                    {
                        _settings.Settings.SelectedMonitor = value.Monitor;
                        _settings.SaveSettings();

                        _wallpaperListVm.SetupDesktop_WallpaperChanged(_wallpaperListVm.CurrentItem, null);
                    }
                }
            }
        }

        private int _selectedWallpaperLayout;
        public int SelectedWallpaperLayout
        {
            get
            {
                return _selectedWallpaperLayout;
            }
            set
            {
                _selectedWallpaperLayout = value;
                OnPropertyChanged();

                if (_settings.Settings.WallpaperArrangement != (WallpaperArrangement)_selectedWallpaperLayout && value != -1)
                {
                    var prevArrangement = _settings.Settings.WallpaperArrangement;
                    _settings.Settings.WallpaperArrangement = (WallpaperArrangement)_selectedWallpaperLayout;
                    _settings.SaveSettings();
                    //SetupDesktop.CloseAllWallpapers();
                    UpdateWallpaper(prevArrangement, _settings.Settings.WallpaperArrangement);
                }
            }
        }

        #region commands

        private bool _canCloseWallpaper = false;
        private RelayCommand _closeWallpaperCommand;
        public RelayCommand CloseWallpaperCommand
        {
            get
            {
                if (_closeWallpaperCommand == null)
                {
                    _closeWallpaperCommand = new RelayCommand(
                        param => CloseWallpaper(SelectedItem),
                        param => _canCloseWallpaper);
                }
                return _closeWallpaperCommand;
            }
        }

        private void CloseWallpaper(ScreenLayoutModel selection)
        {
            if (_settings.Settings.WallpaperArrangement == WallpaperArrangement.Per)
            {
                _desktopCore.CloseWallpaper(selection.Monitor);
            }
            else
            {
                _desktopCore.CloseAllWallpapers();
            }

            selection.PropertyPath = null;
            CanCloseWallpaper();
            CanCustomiseWallpaper();
        }

        private void CanCloseWallpaper()
        {
            bool result = false;
            if (SelectedItem != null)
            {
                foreach (var x in _desktopCore.Wallpapers)
                {
                    if (SelectedItem.Monitor.Equals(x.Monitor))
                    {
                        result = true;
                    }
                }
            }
            _canCloseWallpaper = result;
            RelayCommand.RaiseCanExecuteChanged();
        }

        private bool _canCustomiseWallpaper = false;
        private RelayCommand _customiseWallpaperCommand;
        public RelayCommand CustomiseWallpaperCommand
        {
            get
            {
                if (_customiseWallpaperCommand == null)
                {
                    _customiseWallpaperCommand = new RelayCommand(
                        param => CustomiseWallpaper(SelectedItem),
                        param => _canCustomiseWallpaper);
                }
                return _customiseWallpaperCommand;
            }
        }

        private void CustomiseWallpaper(ScreenLayoutModel selection)
        {
            //only for running wallpapers..
            var items = _desktopCore.Wallpapers.Where(x => x.Metadata.PropertyPath != null).ToList();
            if (items.Count > 0)
            {
                Wallpaper obj = null;
                switch (_settings.Settings.WallpaperArrangement)
                {
                    case WallpaperArrangement.Per:
                        obj = (Wallpaper)(items.Find(x =>
                            MonitorHelper.Compare(x.Monitor, selection.Monitor, MonitorIdentificationMode.DeviceId))?.Metadata);
                        break;
                    case WallpaperArrangement.Span:
                    case WallpaperArrangement.Duplicate:
                        obj = (Wallpaper)items[0].Metadata;
                        break;
                }
                if (obj != null)
                {
                    var settingsWindow = new Core.Cef.PropertiesTrayWindow(obj);
                    settingsWindow.Show();
                }
            }
        }

        private void CanCustomiseWallpaper()
        {
            bool result = false;
            if (SelectedItem != null)
            {
                result = SelectedItem.PropertyPath != null;
            }
            _canCustomiseWallpaper = result;
            RelayCommand.RaiseCanExecuteChanged();
        }

        #endregion //commands

        #region helpers

        public void OnWindowClosing(object? sender, CancelEventArgs e)
        {
            _desktopCore.WallpaperChanged -= SetupDesktop_WallpaperChanged;
        }

        private void UpdateLayout()
        {
            MonitorItems.Clear();
            switch (_settings.Settings.WallpaperArrangement)
            {
                case WallpaperArrangement.Per:
                    {
                        var unsortedScreenItems = new List<ScreenLayoutModel>();
                        foreach (var item in MonitorHelper.GetMonitor())
                        {
                            string propertyFilePath = string.Empty;
                            foreach (var x in _desktopCore.Wallpapers)
                            {
                                if (MonitorHelper.Compare(item, x.Monitor, MonitorIdentificationMode.DeviceId))
                                {
                                    propertyFilePath = x.PropertyCopyPath;
                                }
                            }
                            unsortedScreenItems.Add(
                                new ScreenLayoutModel(item, propertyFilePath, item.DeviceNumber));
                        }

                        foreach (var item in unsortedScreenItems.OrderBy(x => x.Monitor.Bounds.X).ToList())
                        {
                            MonitorItems.Add(item);
                        }
                    }
                    break;
                case WallpaperArrangement.Span:
                    {
                        if (_desktopCore.Wallpapers.Count == 0)
                        {
                            MonitorItems.Add(new ScreenLayoutModel(_settings.Settings.SelectedMonitor, null, "---"));
                        }
                        else
                        {
                            var x = _desktopCore.Wallpapers[0];
                            MonitorItems.Add(new ScreenLayoutModel(_settings.Settings.SelectedMonitor, x.PropertyCopyPath, "---"));
                        }
                    }
                    break;
                case WallpaperArrangement.Duplicate:
                    {
                        if (_desktopCore.Wallpapers.Count == 0)
                        {
                            MonitorItems.Add(new ScreenLayoutModel(_settings.Settings.SelectedMonitor, null, "··"));
                        }
                        else
                        {
                            var x = _desktopCore.Wallpapers[0];
                            MonitorItems.Add(new ScreenLayoutModel(_settings.Settings.SelectedMonitor, x.PropertyCopyPath, "··"));
                        }
                    }
                    break;
            }

            foreach (var item in MonitorItems)
            {
                if (MonitorHelper.Compare(item.Monitor, _settings.Settings.SelectedMonitor, MonitorIdentificationMode.DeviceId))
                {
                    SelectedItem = item;
                    break;
                }
            }
        }

        private void UpdateWallpaper(WallpaperArrangement prev, WallpaperArrangement curr)
        {
            if (_desktopCore.Wallpapers.Count > 0)
            {
                var wallpapers = _desktopCore.Wallpapers.ToList();
                _desktopCore.CloseAllWallpapers();
                if ((prev == WallpaperArrangement.Per && curr == WallpaperArrangement.Span) || (prev == WallpaperArrangement.Per && curr == WallpaperArrangement.Duplicate))
                {
                    var wp = wallpapers.FirstOrDefault(x => MonitorHelper.Compare(x.Monitor, SelectedItem.Monitor, MonitorIdentificationMode.DeviceId)) ?? wallpapers[0];
                    _desktopCore.SetWallpaper(wp.Metadata, MonitorHelper.GetPrimaryMonitor());
                }
                else if ((prev == WallpaperArrangement.Span && curr == WallpaperArrangement.Per) || (prev == WallpaperArrangement.Duplicate && curr == WallpaperArrangement.Per))
                {
                    _desktopCore.SetWallpaper(wallpapers[0].Metadata, SelectedItem.Monitor);
                }
                else if ((prev == WallpaperArrangement.Span && curr == WallpaperArrangement.Duplicate) || (prev == WallpaperArrangement.Duplicate && curr == WallpaperArrangement.Span))
                {
                    _desktopCore.SetWallpaper(wallpapers[0].Metadata, MonitorHelper.GetPrimaryMonitor());
                }
            }
            else
            {
                UpdateLayout();
            }
        }

        #endregion //helpers
    }
}
