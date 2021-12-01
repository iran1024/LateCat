using LateCat.Helpers;
using LateCat.Models;
using LateCat.PoseidonEngine.Abstractions;
using LateCat.PoseidonEngine.Core;
using LateCat.PoseidonEngine.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace LateCat.ViewModels
{
    public class WallpaperListViewModel : ObservableObject
    {
        private readonly List<string> _wallpaperScanFolders = new()
        {
            Path.Combine(Program.WallpaperDir),
            Path.Combine(Program.WallpaperTempDir)
        };

        private readonly IDesktopCore _desktopCore;
        private readonly ISettingsService _settings;

        private int _index = 0;

        public WallpaperListViewModel(ISettingsService userSettings, IDesktopCore desktopCore)
        {
            _settings = userSettings;
            _desktopCore = desktopCore;

            foreach (var item in ScanWallpaperFolders(_wallpaperScanFolders))
            {
                Items.Add(item);
            }

            desktopCore.WallpaperChanged += SetupDesktop_WallpaperChanged;

            var settingsVm = App.Services.GetRequiredService<SettingsViewModel>();

            settingsVm.WallpaperDirChange += SettingsVM_WallpaperDirChange;

            var defaultWallpaper = _items.FirstOrDefault();

            if (defaultWallpaper is not null)
            {
                CurrentItem = defaultWallpaper;
            }
            else
            {
                CurrentItem = Wallpaper.CreateDefaultWallpaper();
                _items.Add(CurrentItem);
            }
        }

        public IWallpaperMetadata? PreviousWallpaper()
        {
            if (_index > 0)
            {
                CrossThreadAccessor.RunAsync(() =>
                    App.Services.GetRequiredService<MainWindow>().Loading.Visibility = Visibility.Visible);

                return CurrentItem = _items[--_index];
            }

            return null;
        }

        private void DisplayTargetWallpaper(int index)
        {
            if (index < 0)
            {
                return;
            }

            CrossThreadAccessor.RunAsync(() =>
                    App.Services.GetRequiredService<MainWindow>().Loading.Visibility = Visibility.Visible);

            CurrentItem = _items[index];
        }

        public IWallpaperMetadata? NextWallpaper()
        {
            if (_index < _items.Count - 1)
            {
                CrossThreadAccessor.RunAsync(() =>
                    App.Services.GetRequiredService<MainWindow>().Loading.Visibility = Visibility.Visible);

                return CurrentItem = _items[++_index];
            }

            return null;
        }

        public void SwitchWallpaper()
        {
            _desktopCore.SetWallpaper(CurrentItem, _settings.Settings.SelectedMonitor);
        }

        #region collections

        private ObservableCollection<IWallpaperMetadata> _items = new();
        public ObservableCollection<IWallpaperMetadata> Items
        {
            get { return _items; }
            set
            {
                if (value != _items)
                {
                    _items = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<IWallpaperMetadata> _itemsFiltered;
        public ObservableCollection<IWallpaperMetadata> ItemsFiltered
        {
            get { return _itemsFiltered; }
            set
            {
                if (value != _itemsFiltered)
                {
                    _itemsFiltered = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                FilterCollection(_searchText);
                OnPropertyChanged();
            }
        }

        private IWallpaperMetadata _currentItem;
        public IWallpaperMetadata CurrentItem
        {
            get
            {
                return _currentItem;
            }
            set
            {
                _currentItem = value;
                OnPropertyChanged();
            }
        }

        private Visibility _placeholder = Visibility.Collapsed;
        public Visibility Placeholder
        {
            get => _placeholder;
            set
            {
                _placeholder = value;
                OnPropertyChanged();
            }
        }

        #endregion //collections

        #region wallpaper operations

        public void WallpaperShowOnDisk(object obj)
        {
            var selection = (IWallpaperMetadata)obj;
            string folderPath;
            if (selection.WallpaperInfo.Type == WallpaperType.Url
            || selection.WallpaperInfo.Type == WallpaperType.VideoStream)
            {
                folderPath = selection.InfoFolderPath;
            }
            else
            {
                folderPath = selection.FilePath;
            }
            FileOperations.OpenFolder(folderPath);
        }

        public IWallpaperMetadata? GetWallpaper(string folder)
        {
            return _items.FirstOrDefault(metadata => metadata.InfoFolderPath.Equals(folder, StringComparison.OrdinalIgnoreCase));
        }

        private void RebuildIndex(bool positive)
        {
            if (positive)
            {
                _index++;
            }
            else
            {
                _index--;
            }
        }

        public async void WallpaperDelete(IWallpaperMetadata metadata)
        {
            var removeIndex = _items.IndexOf(metadata);

            if (removeIndex <= _index)
            {
                RebuildIndex(false);
            }

            _desktopCore.CloseWallpaper(metadata, true);

            var success = await FileOperations.DeleteDirectoryAsync(metadata.InfoFolderPath, 1000, 4000);

            if (success)
            {
                Items.Remove(metadata);

                if (Items.Count == 0)
                {
                    Placeholder = Visibility.Visible;
                    CurrentItem = null;
                }

                if (CurrentItem == metadata)
                {
                    DisplayTargetWallpaper(_index);
                }

                try
                {
                    if (string.IsNullOrEmpty(metadata.InfoFolderPath))
                        return;

                    string[] wpdataDir = Directory.GetDirectories(Program.WallpaperDataDir);
                    var wpFolderName = new DirectoryInfo(metadata.InfoFolderPath).Name;
                    for (int i = 0; i < wpdataDir.Length; i++)
                    {
                        var item = new DirectoryInfo(wpdataDir[i]).Name;
                        if (wpFolderName.Equals(item, StringComparison.Ordinal))
                        {
                            _ = FileOperations.DeleteDirectoryAsync(wpdataDir[i], 1000, 4000);
                            break;
                        }
                    }
                }
                catch
                { }
            }
        }

        public void WallpaperVideoConvert(object obj)
        {
            var selection = (IWallpaperMetadata)obj;
            var model = new Wallpaper(selection.WallpaperInfo, selection.InfoFolderPath, WallpaperProcessStatus.VideoConvert);
            _desktopCore.SetWallpaper(model, _settings.Settings.SelectedMonitor);
        }

        #endregion //wallpaper operations

        #region helpers

        public void AddWallpaper(string path, WallpaperType wpType, WallpaperProcessStatus dataType, IMonitor monitor, string? cmdArgs = null)
        {
            var dir = Path.Combine(Program.WallpaperTempDir, Path.GetRandomFileName());

            if (dataType == WallpaperProcessStatus.Processing ||
                dataType == WallpaperProcessStatus.CmdImport ||
                dataType == WallpaperProcessStatus.MultiImport)
            {
                try
                {
                    Directory.CreateDirectory(dir);
                }
                catch
                {
                    return;
                }

                var data = new WallpaperInfo()
                {
                    Title = Properties.Resources.TextProcessingWallpaper + "...",
                    Type = wpType,
                    IsAbsolutePath = true,
                    FileName = path,
                    Arguments = cmdArgs ?? string.Empty
                };

                var model = new Wallpaper(data, dir, dataType);

                if (_items.Count - 1 == _index)
                {
                    RebuildIndex(true);
                }

                Items.Insert(_items.Count - 1, model);

                if (Placeholder == Visibility.Visible)
                {
                    Placeholder = Visibility.Collapsed;
                }

                _desktopCore.SetWallpaper(model, monitor);
            }
        }

        public void AddWallpaper(string folderPath)
        {
            var libItem = ScanWallpaperFolder(folderPath);
            if (libItem != null)
            {
                var binarySearchIndex = BinarySearch(Items, libItem.Title);

                if (binarySearchIndex <= _index)
                {
                    RebuildIndex(true);
                }

                Items.Insert(binarySearchIndex, libItem);

                if (Placeholder == Visibility.Visible)
                {
                    Placeholder = Visibility.Collapsed;
                    CurrentItem = _items.FirstOrDefault()!;
                }
            }
        }

        public void EditWallpaper(IWallpaperMetadata obj)
        {
            _desktopCore.CloseWallpaper(obj, true);
            Items.Remove(obj);
            obj.Status = WallpaperProcessStatus.Edit;
            Items.Insert(0, obj);
            _desktopCore.SetWallpaper(obj, MonitorHelper.GetPrimaryMonitor());
        }

        private List<Wallpaper> ScanWallpaperFolders(List<string> folderPaths)
        {
            var dir = new List<string[]>();

            for (int i = 0; i < folderPaths.Count; i++)
            {
                try
                {
                    dir.Add(Directory.GetDirectories(folderPaths[i], "*", SearchOption.TopDirectoryOnly));
                }
                catch
                {

                }
            }

            var tmpLibItems = new List<Wallpaper>();

            for (int i = 0; i < dir.Count; i++)
            {
                for (int j = 0; j < dir[i].Length; j++)
                {
                    var currDir = dir[i][j];
                    var libItem = ScanWallpaperFolder(currDir);
                    if (libItem != null)
                    {
                        tmpLibItems.Add(libItem);
                    }
                }
            }

            return SortWallpapers(tmpLibItems);
        }

        private Wallpaper ScanWallpaperFolder(string folderPath)
        {
            if (File.Exists(Path.Combine(folderPath, "WallpaperInfo.json")))
            {
                WallpaperInfo info = null;
                try
                {
                    info = Json<WallpaperInfo>.LoadData(Path.Combine(folderPath, "WallpaperInfo.json"));
                }
                catch
                {

                }

                if (info != null)
                {
                    if (info.Type == WallpaperType.VideoStream || info.Type == WallpaperType.Url)
                    {
                        return new Wallpaper(info, folderPath, WallpaperProcessStatus.Ready);
                    }
                    else
                    {
                        if (info.IsAbsolutePath)
                        {

                        }
                        else
                        {

                        }
                        return new Wallpaper(info, folderPath, WallpaperProcessStatus.Ready);
                    }
                }
            }
            else
            {

            }
            return null;
        }

        private List<Wallpaper> SortWallpapers(List<Wallpaper> data)
        {
            try
            {
                return data.OrderBy(x => x.WallpaperInfo.Title).ToList();
            }
            catch (ArgumentNullException)
            {
                return null;
            }
        }

        public void SortLibraryItem(Wallpaper item)
        {
            Items.Remove(item);
            var binarySearchIndex = BinarySearch(Items, item.Title);

            Items.Insert(binarySearchIndex, item);
        }

        private void FilterCollection(string str)
        {
            var tmpFilter = Items.Where(item => item.WallpaperInfo.Title.Contains(str, StringComparison.OrdinalIgnoreCase)).ToList();

            for (int i = 0; i < ItemsFiltered.Count; i++)
            {
                var item = ItemsFiltered[i];
                if (!tmpFilter.Contains(item))
                {
                    ItemsFiltered.Remove(item);
                }
            }

            for (int i = 0; i < tmpFilter.Count; i++)
            {
                var item = tmpFilter[i];
                if (!ItemsFiltered.Contains(item))
                {
                    var index = BinarySearch(ItemsFiltered, item.Title);
                    ItemsFiltered.Insert(index, item);
                }
            }
        }

        private static int BinarySearch(ObservableCollection<IWallpaperMetadata> item, string x)
        {
            if (x is null)
            {
                throw new ArgumentNullException(nameof(x));
            }

            int l = 0, r = item.Count - 1, m, res;
            while (l <= r)
            {
                m = (l + r) / 2;

                res = string.Compare(x, item[m].Title);

                if (res == 0)
                    return m;

                if (res > 0)
                    l = m + 1;

                else
                    r = m - 1;
            }
            return l;
        }

        #endregion //helpers

        #region setupdesktop

        public void SetupDesktop_WallpaperChanged(object? sender, EventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
            new System.Threading.ThreadStart(delegate
            {
                if (sender is IWallpaperMetadata metadata)
                {
                    _desktopCore.SetWallpaper(metadata, _settings.Settings.SelectedMonitor);
                }

                //CurrentItem = _settings.Settings.WallpaperArrangement == WallpaperArrangement.Span && _desktopCore.Wallpapers.Count > 0 ?
                //    _desktopCore.Wallpapers[0].Metadata :
                //    _desktopCore.Wallpapers.FirstOrDefault(wp => _settings.Settings.SelectedMonitor.Equals(wp.Monitor))?.Metadata;
            }));
        }

        #endregion //setupdesktop

        #region settings changed

        private void SettingsVM_WallpaperDirChange(object? sender, string dir)
        {
            Items.Clear();
            _wallpaperScanFolders.Clear();
            _wallpaperScanFolders.Add(Path.Combine(dir, "wallpapers"));
            _wallpaperScanFolders.Add(Path.Combine(dir, ".latecat", "temp"));

            foreach (var item in ScanWallpaperFolders(_wallpaperScanFolders))
            {
                Items.Add(item);
            }
        }

        public void WallpaperDirectoryUpdate()
        {
            Items.Clear();
            foreach (var item in ScanWallpaperFolders(_wallpaperScanFolders))
            {
                Items.Add(item);
            }
        }

        #endregion //settings changed
    }
}
