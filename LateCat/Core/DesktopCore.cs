using LateCat.Helpers;
using LateCat.PoseidonEngine;
using LateCat.PoseidonEngine.Abstractions;
using LateCat.PoseidonEngine.Core;
using LateCat.PoseidonEngine.Models;
using LateCat.ViewModels;
using LateCat.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace LateCat.Core
{
    internal class DesktopCore : IDesktopCore
    {
        private readonly List<IWallpaper> _wallpapers = new(2);
        private IntPtr _progman, _workerw;
        private bool _isInitialized = false;
        private bool _disposed;
        private readonly List<IWallpaper> _wallpapersPending = new(2);
        private readonly List<WallpaperLayout> _wallpapersDisconnected = new();

        public event EventHandler WallpaperChanged;
        public event EventHandler WallpaperReset;

        private readonly ISettingsService _settings;
        private readonly IWallpaperFactory _wallpaperFactory;
        private readonly ITaskbarOperator _taskbarOperator;
        private readonly IWatchdogService _watchDog;

        public ReadOnlyCollection<IWallpaper> Wallpapers => _wallpapers.AsReadOnly();

        public IntPtr WorkerW => _workerw;

        public DesktopCore(ISettingsService userSettings, ITaskbarOperator ttbService, IWatchdogService watchdog, IWallpaperFactory wallpaperFactory)
        {
            _settings = userSettings;
            _taskbarOperator = ttbService;
            _watchDog = watchdog;

            _wallpaperFactory = wallpaperFactory;

            MonitorHelper.MonitorUpdated += MonitorSettingsChanged_Hwnd;
            WallpaperChanged += SetupDesktop_WallpaperChanged;
        }

        public void SetWallpaper(IWallpaperMetadata wallpaper, IMonitor monitor)
        {
            if (!_isInitialized)
            {
                if (SystemParameters.HighContrast)
                {
                    MessageBox.Show("检测到高对比度模式，某些功能可能无法正常工作!");
                }

                _progman = Win32.FindWindow("Progman", "Program Manager");

                Win32.SendMessageTimeout(_progman,
                                       0x052C,
                                       new IntPtr(0xD),
                                       new IntPtr(0x1),
                                       Win32.SendMessageTimeoutFlags.SMTO_NORMAL,
                                       1000,
                                       out var result);

                _workerw = IntPtr.Zero;

                Win32.EnumWindows(new Win32.EnumWindowsProc((tophandle, topparamhandle) =>
                {
                    var p = Win32.FindWindowEx(tophandle,
                                                IntPtr.Zero,
                                                "SHELLDLL_DefView",
                                                IntPtr.Zero);

                    if (p != IntPtr.Zero)
                    {
                        _workerw = Win32.FindWindowEx(IntPtr.Zero,
                                                       tophandle,
                                                       "WorkerW",
                                                       IntPtr.Zero);
                    }

                    return true;
                }), IntPtr.Zero);

                if (Equals(_workerw, IntPtr.Zero))
                {
                    MessageBox.Show(Properties.Resources.ExceptionWorkerWSetupFail, Properties.Resources.TextError, MessageBoxButton.OK, MessageBoxImage.Exclamation);

                    WallpaperChanged?.Invoke(this, EventArgs.Empty);

                    return;
                }
                else
                {
                    _isInitialized = true;

                    WallpaperReset?.Invoke(this, EventArgs.Empty);

                    App.Services.GetRequiredService<IPlayback>().Start();

                    _watchDog.Start();
                }
            }

            if (!MonitorHelper.Exists(monitor, MonitorIdentificationMode.DeviceId))
            {
                return;
            }
            else if (_wallpapersPending.Exists(x => monitor.Equals(x.Monitor)))
            {
                return;
            }
            else if (!(wallpaper.WallpaperInfo.IsAbsolutePath ?
                wallpaper.WallpaperInfo.Type == WallpaperType.Url || wallpaper.WallpaperInfo.Type == WallpaperType.VideoStream || File.Exists(wallpaper.FilePath) :
                wallpaper.FilePath != null))
            {
                _ = Task.Run(() =>
                    MessageBox.Show(Properties.Resources.TextFileNotFound, Properties.Resources.TextError + " " + Properties.Resources.TitleAppName, MessageBoxButton.OK, MessageBoxImage.Information));

                WallpaperChanged?.Invoke(this, EventArgs.Empty);
                return;
            }

            try
            {
                IWallpaper instance = _wallpaperFactory.CreateWallpaper(wallpaper, monitor, _settings);

                _ = Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new ThreadStart(delegate
                {
                    wallpaper.ItemStartup = true;
                }));

                instance.WindowInitialized += WallpaperInitialized;
                _wallpapersPending.Add(instance);
                instance.Show();
            }
            catch
            {
                _ = Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new ThreadStart(delegate
                {
                    if (wallpaper.Status == WallpaperProcessStatus.Processing)
                    {
                        App.Services.GetRequiredService<WallpaperListViewModel>().WallpaperDelete(wallpaper);
                    }

                    WallpaperChanged?.Invoke(this, EventArgs.Empty);
                }));
                _ = Task.Run(() => MessageBox.Show(Properties.Resources.TextFeatureMissing, Properties.Resources.TitleAppName, MessageBoxButton.OK, MessageBoxImage.Information));
            }
        }


        private readonly SemaphoreSlim semaphoreSlimWallpaperInitLock = new(1, 1);

        private async void WallpaperInitialized(object? sender, WindowInitializedArgs e)
        {
            await semaphoreSlimWallpaperInitLock.WaitAsync();
            IWallpaper? wallpaper = null;
            bool reloadRequired = false;
            try
            {
                wallpaper = (IWallpaper)sender!;
                _wallpapersPending.Remove(wallpaper);
                wallpaper.WindowInitialized -= WallpaperInitialized;
                await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new ThreadStart(delegate
                {
                    wallpaper.Metadata.ItemStartup = false;
                }));
                if (e.Success)
                {
                    switch (wallpaper.Metadata.Status)
                    {
                        case WallpaperProcessStatus.Processing:
                        case WallpaperProcessStatus.CmdImport:
                        case WallpaperProcessStatus.MultiImport:
                        case WallpaperProcessStatus.Edit:

                            var type = wallpaper.Metadata.Status;

                            var tcs = new TaskCompletionSource<object>();
                            var thread = new Thread(() =>
                            {
                                try
                                {
                                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new ThreadStart(delegate
                                    {
                                        var appWindow = App.Services.GetRequiredService<MainWindow>();
                                        var pWindow = new WallpaperPreviewView(wallpaper);
                                        if (type == WallpaperProcessStatus.MultiImport)
                                        {
                                            pWindow.Topmost = true;
                                            pWindow.ShowActivated = true;
                                            if (appWindow.IsVisible)
                                            {
                                                pWindow.Left = appWindow.Left;
                                                pWindow.Top = appWindow.Top;
                                                pWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                                            }
                                            pWindow.Closed += (s, a) => tcs.SetResult(0);
                                            pWindow.Show();
                                        }
                                        else if (type == WallpaperProcessStatus.CmdImport)
                                        {
                                            pWindow.Topmost = true;
                                            pWindow.ShowActivated = true;
                                            pWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                                            pWindow.Closed += (s, a) => tcs.SetResult(0);
                                            pWindow.Show();
                                        }
                                        else
                                        {
                                            if (appWindow.IsVisible)
                                            {
                                                pWindow.Owner = appWindow;
                                                pWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                                            }
                                            pWindow.ShowDialog();
                                            tcs.SetResult(0);
                                        }
                                    }));
                                }
                                catch (Exception e)
                                {
                                    tcs.SetException(e);
                                }
                            });
                            thread.SetApartmentState(ApartmentState.STA);
                            thread.Start();
                            await tcs.Task;

                            if (!File.Exists(Path.Combine(wallpaper.Metadata.InfoFolderPath, "WallpaperInfo.json")))
                            {
                                wallpaper.Terminate();
                                DesktopUtil.RefreshDesktop(Program.OriginalDesktopWallpaperPath);
                                await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new ThreadStart(delegate
                                {
                                    App.Services.GetRequiredService<WallpaperListViewModel>().WallpaperDelete(wallpaper.Metadata);
                                }));

                                return;
                            }
                            else
                            {
                                if (type == WallpaperProcessStatus.Edit)
                                {
                                    wallpaper.Terminate();
                                    return;
                                }
                                else if (type == WallpaperProcessStatus.MultiImport)
                                {
                                    wallpaper.Terminate();

                                    WallpaperChanged?.Invoke(this, EventArgs.Empty);
                                    return;
                                }
                            }
                            break;
                        case WallpaperProcessStatus.Installing:
                            break;
                        case WallpaperProcessStatus.Downloading:
                            break;
                        case WallpaperProcessStatus.Ready:
                            break;
                        case WallpaperProcessStatus.VideoConvert:
                            wallpaper.Terminate();
                            DesktopUtil.RefreshDesktop();

                            return;
                        default:
                            break;
                    }

                    reloadRequired = wallpaper.WallpaperType == WallpaperType.Web ||
                        wallpaper.WallpaperType == WallpaperType.WebAudio ||
                        wallpaper.WallpaperType == WallpaperType.Url;

                    if (!MonitorHelper.IsMultiMonitor())
                    {
                        CloseAllWallpapers(false, true);
                        SetWallpaperPerMonitor(wallpaper.Handle, wallpaper.Monitor);
                    }
                    else
                    {
                        switch (_settings.Settings.WallpaperArrangement)
                        {
                            case WallpaperArrangement.Per:
                                CloseWallpaper(wallpaper.Monitor, false);
                                SetWallpaperPerMonitor(wallpaper.Handle, wallpaper.Monitor);
                                break;
                            case WallpaperArrangement.Span:
                                CloseAllWallpapers(false, false);
                                SetWallpaperSpanMonitor(wallpaper.Handle);
                                break;
                            case WallpaperArrangement.Duplicate:
                                CloseWallpaper(wallpaper.Monitor, false, false);
                                SetWallpaperDuplicateMonitor(wallpaper);
                                break;
                        }
                    }

                    if (reloadRequired)
                    {
                        wallpaper.SetPlaybackPos(0, PlaybackPosType.Absolute);
                    }

                    if (wallpaper.Proc != null)
                    {
                        _watchDog.Add(wallpaper.Proc.Id);
                    }

                    var thumbRequiredAvgColor = (_settings.Settings.SystemTaskbarTheme == TaskbarTheme.Wallpaper || _settings.Settings.SystemTaskbarTheme == TaskbarTheme.WallpaperFluent) &&
                        (!MonitorHelper.IsMultiMonitor() || _settings.Settings.WallpaperArrangement == WallpaperArrangement.Span || MonitorHelper.Compare(wallpaper.Monitor, MonitorHelper.GetPrimaryMonitor(), MonitorIdentificationMode.DeviceId));

                    if (_settings.Settings.DesktopAutoWallpaper || thumbRequiredAvgColor)
                    {
                        try
                        {
                            int maxIterations = 50;

                            for (int i = 1; i <= maxIterations; i++)
                            {
                                if (i == maxIterations)
                                    throw new Exception("Timed out..");

                                if (wallpaper.IsLoaded)
                                    break;

                                await Task.Delay(20);
                            }

                            var imgPath = Path.Combine(Constants.Paths.TempDir, Path.GetRandomFileName() + ".jpg");

                            await wallpaper.Capture(imgPath);

                            if (!File.Exists(imgPath))
                            {
                                throw new FileNotFoundException();
                            }

                            try
                            {
                                var color = await Task.Run(() => _taskbarOperator.GetAverageColor(imgPath));
                                _taskbarOperator.SetAccentColor(color);
                            }
                            catch
                            {

                            }

                            if (_settings.Settings.DesktopAutoWallpaper)
                            {
                                if (MonitorHelper.IsMultiMonitor())
                                {
                                    var desktop = (INativeWallpaper)new NativeWallpaperInterop();
                                    DesktopWallpaperPosition scaler = DesktopWallpaperPosition.Fill;
                                    switch (_settings.Settings.WallpaperScaling)
                                    {
                                        case WallpaperScaler.None:
                                            scaler = DesktopWallpaperPosition.Center;
                                            break;
                                        case WallpaperScaler.Fill:
                                            scaler = DesktopWallpaperPosition.Stretch;
                                            break;
                                        case WallpaperScaler.Uniform:
                                            scaler = DesktopWallpaperPosition.Fit;
                                            break;
                                        case WallpaperScaler.UniformToFill:
                                            scaler = DesktopWallpaperPosition.Fill;
                                            break;
                                    }
                                    desktop.SetPosition(_settings.Settings.WallpaperArrangement == WallpaperArrangement.Span ? DesktopWallpaperPosition.Span : scaler);
                                    desktop.SetWallpaper(_settings.Settings.WallpaperArrangement == WallpaperArrangement.Span ? null : wallpaper.Monitor.DeviceId, imgPath);
                                }
                                else
                                {
                                    _ = Win32.SystemParametersInfo(Win32.SPI_SETDESKWALLPAPER, 0, imgPath, Win32.SPIF_UPDATEINIFILE | Win32.SPIF_SENDWININICHANGE);
                                }
                            }
                        }
                        catch
                        {

                        }
                    }

                    _wallpapers.Add(wallpaper);
                    WallpaperChanged?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    wallpaper.Terminate();
                    WallpaperChanged?.Invoke(this, EventArgs.Empty);

                    var appWindow = App.Services.GetRequiredService<MainWindow>();
                    if (appWindow.IsVisible)
                    {
                        MessageBox.Show(Properties.Resources.ExceptionGeneral, Properties.Resources.TitleAppName, MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    if (!File.Exists(Path.Combine(wallpaper.Metadata.InfoFolderPath, "WallpaperInfo.json")))
                    {
                        await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new ThreadStart(delegate
                        {
                            App.Services.GetRequiredService<WallpaperListViewModel>().WallpaperDelete(wallpaper.Metadata);
                        }));
                    }
                }
            }
            catch (Exception ex)
            {

                wallpaper?.Terminate();
                WallpaperChanged?.Invoke(this, EventArgs.Empty);
            }
            finally
            {
                semaphoreSlimWallpaperInitLock.Release();
            }
        }

        private void SetWallpaperPerMonitor(IntPtr handle, IMonitor targetDisplay)
        {
            var prct = new Win32.RECT();

            if (!Win32.SetWindowPos(handle, 1, targetDisplay.Bounds.X, targetDisplay.Bounds.Y, targetDisplay.Bounds.Width, targetDisplay.Bounds.Height, 0x0010))
            {

            }

            _ = Win32.MapWindowPoints(handle, _workerw, ref prct, 2);

            SetParentWorkerW(handle);

            if (!Win32.SetWindowPos(handle, 1, prct.Left, prct.Top, targetDisplay.Bounds.Width, targetDisplay.Bounds.Height, 0x0010))
            {

            }

            DesktopUtil.RefreshDesktop(Program.OriginalDesktopWallpaperPath);
        }

        private void SetWallpaperSpanMonitor(IntPtr handle)
        {
            _ = Win32.GetWindowRect(_workerw, out Win32.RECT prct);
            SetParentWorkerW(handle);
            // 跨显示器
            if (!Win32.SetWindowPos(handle, 1, 0, 0, prct.Right - prct.Left, prct.Bottom - prct.Top, 0x0010))
            {

            }

            DesktopUtil.RefreshDesktop(Program.OriginalDesktopWallpaperPath);
        }

        private void SetWallpaperDuplicateMonitor(IWallpaper wallpaper)
        {
            SetWallpaperPerMonitor(wallpaper.Handle, wallpaper.Monitor);

            var remainingMonitors = MonitorHelper.GetMonitor().ToList();
            var currDuplicates = _wallpapers.FindAll(x => x.Metadata == wallpaper.Metadata);
            remainingMonitors.RemoveAll(x => MonitorHelper.Compare(wallpaper.Monitor, x, MonitorIdentificationMode.DeviceId) ||
                currDuplicates.FindIndex(y => MonitorHelper.Compare(y.Monitor, x, MonitorIdentificationMode.DeviceId)) != -1);
            if (remainingMonitors.Count != 0)
            {
                SetWallpaper(wallpaper.Metadata, remainingMonitors[0]);
            }
            else
            {
                var mpvFix = (wallpaper.WallpaperType == WallpaperType.Video || wallpaper.WallpaperType == WallpaperType.VideoStream) &&
                    _settings.Settings.VideoPlayer == MediaPlayerType.MPV;

                _wallpapers.ForEach(x =>
                {
                    if (mpvFix)
                    {
                        x.SendMessage("{\"command\":[\"set_property\",\"aid\",\"no\"]}\n");
                    }

                    x.SetPlaybackPos(0, PlaybackPosType.Absolute);
                });

                if (mpvFix)
                {
                    wallpaper.SetPlaybackPos(0, PlaybackPosType.Absolute);
                }
            }
        }

        public void ResetWallpaper()
        {
            _isInitialized = false;

            App.Services.GetRequiredService<IPlayback>().Stop();

            if (Wallpapers.Count > 0)
            {
                var originalWallpapers = Wallpapers.ToList();

                CloseAllWallpapers(true);

                foreach (var item in originalWallpapers)
                {
                    SetWallpaper(item.Metadata, item.Monitor);
                    if (_settings.Settings.WallpaperArrangement == WallpaperArrangement.Duplicate)
                        break;
                }
            }

        }

        static readonly object _layoutWriteLock = new();
        private void SetupDesktop_WallpaperChanged(object? sender, EventArgs e)
        {
            lock (_layoutWriteLock)
            {
                SaveWallpaperLayout();
            }
        }

        private void SaveWallpaperLayout()
        {
            var layout = new List<WallpaperLayout>();
            _wallpapers.ForEach(wallpaper =>
            {
                layout.Add(new WallpaperLayout(
                        (PoseidonMonitor)wallpaper.Monitor,
                        wallpaper.Metadata.InfoFolderPath));
            });

            if (_settings.Settings.WallpaperArrangement == WallpaperArrangement.Per)
            {
                layout.AddRange(_wallpapersDisconnected);
            }
            /*
            layout.AddRange(wallpapersDisconnected.Except(wallpapersDisconnected.FindAll(
                layout => Wallpapers.FirstOrDefault(wp => MonitorHelper.MonitorCompare(layout.PoseidonMonitor, wp.GetMonitor(), MonitorIdentificationMode.DeviceId)) != null)));
            */

            try
            {
                Json<List<WallpaperLayout>>.StoreData(Constants.Paths.LayoutPath, layout);
            }
            catch (Exception e)
            {

            }
        }

        static readonly SemaphoreSlim semaphoreSlimDisplaySettingsChangedLock = new SemaphoreSlim(1, 1);

        private async void MonitorSettingsChanged_Hwnd(object? sender, EventArgs e)
        {
            await semaphoreSlimDisplaySettingsChangedLock.WaitAsync();

            try
            {
                RefreshWallpaper();

                await RestoreDisconnectedWallpapers();
            }
            finally
            {
                semaphoreSlimDisplaySettingsChangedLock.Release();
            }
        }

        private void RefreshWallpaper()
        {
            try
            {
                var allMonitors = MonitorHelper.GetMonitor();
                var orphanWallpapers = _wallpapers.FindAll(
                    wallpaper => allMonitors.Find(
                        Monitor => MonitorHelper.Compare(wallpaper.Monitor, Monitor, MonitorIdentificationMode.DeviceId)) == null);

                _settings.Settings.SelectedMonitor =
                    allMonitors.Find(x => MonitorHelper.Compare(_settings.Settings.SelectedMonitor, x, MonitorIdentificationMode.DeviceId)) ??
                    MonitorHelper.GetPrimaryMonitor();

                _settings.SaveSettings();

                switch (_settings.Settings.WallpaperArrangement)
                {
                    case WallpaperArrangement.Per:

                        if (orphanWallpapers.Count != 0)
                        {
                            orphanWallpapers.ForEach(x =>
                            {
                                x.Close();
                            });

                            var newOrphans = orphanWallpapers.FindAll(
                                oldOrphan => _wallpapersDisconnected.Find(
                                    newOrphan => MonitorHelper.Compare(newOrphan.Monitor, oldOrphan.Monitor, MonitorIdentificationMode.DeviceId)) == null);
                            foreach (var item in newOrphans)
                            {
                                _wallpapersDisconnected.Add(new WallpaperLayout((PoseidonMonitor)item.Monitor, item.Metadata.InfoFolderPath));
                            }
                            _wallpapers.RemoveAll(x => orphanWallpapers.Contains(x));
                        }
                        break;
                    case WallpaperArrangement.Duplicate:
                        if (orphanWallpapers.Count != 0)
                        {
                            orphanWallpapers.ForEach(x =>
                            {
                                x.Close();
                            });
                            _wallpapers.RemoveAll(x => orphanWallpapers.Contains(x));
                        }
                        break;
                    case WallpaperArrangement.Span:

                        break;
                }

                UpdateWallpaperRect();
            }
            catch
            {

            }
            finally
            {
                WallpaperChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        private void UpdateWallpaperRect()
        {
            try
            {
                App.Services.GetRequiredService<IPlayback>().Stop();
                if (MonitorHelper.IsMultiMonitor() && _settings.Settings.WallpaperArrangement == WallpaperArrangement.Span)
                {
                    if (_wallpapers.Count != 0)
                    {
                        Wallpapers[0].Play();
                        var MonitorArea = MonitorHelper.GetVirtualMonitorBounds();

                        Wallpapers[0].Monitor = MonitorHelper.GetPrimaryMonitor();
                        Win32.SetWindowPos(Wallpapers[0].Handle, 1, 0, 0, MonitorArea.Width, MonitorArea.Height, 0x0010);
                    }
                }
                else
                {
                    int i;
                    foreach (var Monitor in MonitorHelper.GetMonitor().ToList())
                    {
                        if ((i = _wallpapers.FindIndex(x => MonitorHelper.Compare(Monitor, x.Monitor, MonitorIdentificationMode.DeviceId))) != -1)
                        {
                            Wallpapers[i].Play();

                            Wallpapers[i].Monitor = Monitor;

                            var MonitorArea = MonitorHelper.GetVirtualMonitorBounds();
                            if (!Win32.SetWindowPos(Wallpapers[i].Handle,
                                                            1,
                                                            (Monitor.Bounds.X - MonitorArea.Location.X),
                                                            (Monitor.Bounds.Y - MonitorArea.Location.Y),
                                                            (Monitor.Bounds.Width),
                                                            (Monitor.Bounds.Height),
                                                            0x0010))
                            {

                            }
                        }
                    }
                }
                DesktopUtil.RefreshDesktop();
            }
            finally
            {
                App.Services.GetRequiredService<IPlayback>().Start();
            }
        }

        private async Task RestoreDisconnectedWallpapers()
        {
            try
            {
                switch (_settings.Settings.WallpaperArrangement)
                {
                    case WallpaperArrangement.Per:
                        var wallpapersToRestore = _wallpapersDisconnected.FindAll(wallpaper => MonitorHelper.GetMonitor().FirstOrDefault(
                            Monitor => MonitorHelper.Compare(wallpaper.Monitor, Monitor, MonitorIdentificationMode.DeviceId)) != null);
                        await RestoreWallpaper(wallpapersToRestore);
                        break;
                    case WallpaperArrangement.Span:
                        break;
                    case WallpaperArrangement.Duplicate:
                        if ((MonitorHelper.MonitorCount() > Wallpapers.Count) && Wallpapers.Count != 0)
                        {
                            var newMonitor = MonitorHelper.GetMonitor().FirstOrDefault(Monitor => Wallpapers.FirstOrDefault(
                                wp => MonitorHelper.Compare(wp.Monitor, Monitor, MonitorIdentificationMode.DeviceId)) == null);
                            if (newMonitor != null)
                            {
                                SetWallpaper(Wallpapers[0].Metadata, newMonitor);
                            }
                        }

                        break;
                }
            }
            catch
            {

            }
        }

        private async Task RestoreWallpaperFromLayout(string wallpaperLayoutPath)
        {
            try
            {
                var wallpaperLayout = Json<List<WallpaperLayout>>.LoadData(wallpaperLayoutPath);
                if (_settings.Settings.WallpaperArrangement == WallpaperArrangement.Span ||
                    _settings.Settings.WallpaperArrangement == WallpaperArrangement.Duplicate)
                {
                    if (wallpaperLayout.Count != 0)
                    {
                        await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new ThreadStart(delegate
                        {
                            var libraryItem = App.Services.GetRequiredService<WallpaperListViewModel>().Items.FirstOrDefault(x => x.InfoFolderPath.Equals(wallpaperLayout[0].ConfigPath));

                            if (libraryItem != null)
                            {
                                SetWallpaper(libraryItem, MonitorHelper.GetPrimaryMonitor());
                            }
                        }));
                    }
                }
                else if (_settings.Settings.WallpaperArrangement == WallpaperArrangement.Per)
                {
                    await RestoreWallpaper(wallpaperLayout);
                }
            }
            catch (Exception e)
            {

            }
        }

        private async Task RestoreWallpaper(List<WallpaperLayout> wallpaperLayout)
        {
            await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new ThreadStart(delegate
            {
                foreach (var layout in wallpaperLayout)
                {
                    var libraryItem = App.Services.GetRequiredService<WallpaperListViewModel>().Items.FirstOrDefault(x => x.InfoFolderPath.Equals(layout.ConfigPath));
                    var Monitor = MonitorHelper.GetMonitor(layout.Monitor.DeviceId, layout.Monitor.DeviceName,
                        layout.Monitor.Bounds, layout.Monitor.WorkingArea, MonitorIdentificationMode.DeviceId);
                    if (libraryItem == null)
                    {
                        _wallpapersDisconnected.Remove(layout);
                    }
                    else if (Monitor == null)
                    {
                        if (!_wallpapersDisconnected.Contains(layout))
                        {
                            _wallpapersDisconnected.Add(new WallpaperLayout((PoseidonMonitor)layout.Monitor, layout.ConfigPath));
                        }
                    }
                    else
                    {
                        SetWallpaper(libraryItem, Monitor);
                        _wallpapersDisconnected.Remove(layout);
                    }
                }
            }));
        }

        public void RestoreWallpaper()
        {
            _ = RestoreWallpaperFromLayout(Constants.Paths.LayoutPath);
        }

        public void RestoreDeskWallpaper()
        {
            CloseAllWallpapersAndRestoreDeskWallpaper();
        }

        public void CloseAllWallpapers(bool terminate = false)
        {
            CloseAllWallpapers(true, terminate);
        }

        private void CloseAllWallpapersAndRestoreDeskWallpaper()
        {
            if (Wallpapers.Count > 0)
            {
                _wallpapers.ForEach(x => x.Terminate());

                _wallpapers.Clear();
                _watchDog.Clear();
            }
            else
            {
                DesktopUtil.RefreshDesktop(Program.OriginalDesktopWallpaperPath);
            }
        }

        private void CloseAllWallpapers(bool fireEvent, bool terminate)
        {
            if (Wallpapers.Count > 0)
            {
                if (terminate)
                {
                    _wallpapers.ForEach(x => x.Terminate());
                }
                else
                {
                    _wallpapers.ForEach(x => x.Close());
                }
                _wallpapers.Clear();
                _watchDog.Clear();

                if (fireEvent)
                {
                    WallpaperChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public void CloseWallpaper(IMonitor display, bool terminate = false)
        {
            CloseWallpaper(display, true, terminate);
        }

        private void CloseWallpaper(IMonitor display, bool fireEvent, bool terminate)
        {
            var tmp = _wallpapers.FindAll(x => x.Monitor.Equals(display));
            if (tmp.Count > 0)
            {
                tmp.ForEach(x =>
                {
                    if (x.Proc != null)
                    {
                        _watchDog.Remove(x.Proc.Id);
                    }

                    if (terminate)
                    {
                        x.Terminate();
                    }
                    else
                    {
                        x.Close();
                    }
                });
                _wallpapers.RemoveAll(x => tmp.Contains(x));

                if (fireEvent)
                {
                    WallpaperChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public void CloseWallpaper(WallpaperType type, bool terminate = false)
        {
            var tmp = _wallpapers.FindAll(x => x.WallpaperType == type);
            if (tmp.Count > 0)
            {
                tmp.ForEach(x =>
                {
                    if (x.Proc != null)
                    {
                        _watchDog.Remove(x.Proc.Id);
                    }

                    if (terminate)
                    {
                        x.Terminate();
                    }
                    else
                    {
                        x.Close();
                    }
                });

                _wallpapers.RemoveAll(x => tmp.Contains(x));
                WallpaperChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void CloseWallpaper(IWallpaperMetadata wp, bool terminate = false)
        {
            CloseWallpaper(wp, true, terminate);
        }

        private void CloseWallpaper(IWallpaperMetadata wp, bool fireEvent, bool terminate)
        {
            var tmp = _wallpapers.FindAll(x => x.Metadata == wp);
            if (tmp.Count > 0)
            {
                tmp.ForEach(x =>
                {
                    if (x.Proc != null)
                    {
                        _watchDog.Remove(x.Proc.Id);
                    }

                    if (terminate)
                    {
                        x.Terminate();
                    }
                    else
                    {
                        x.Close();
                    }
                });
                _wallpapers.RemoveAll(x => tmp.Contains(x));

                if (fireEvent)
                {
                    WallpaperChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public void SendMessageWallpaper(IWallpaperMetadata wp, IPCMessage msg)
        {
            _wallpapers.ForEach(x =>
            {
                if (x.Metadata == wp)
                {
                    x.SendMessage(msg);
                }
            });
        }

        public void SendMessageWallpaper(IMonitor display, IWallpaperMetadata wp, IPCMessage msg)
        {
            _wallpapers.ForEach(x =>
            {
                if (x.Monitor.Equals(display) && wp == x.Metadata)
                    x.SendMessage(msg);
            });
        }

        public void SeekWallpaper(IWallpaperMetadata wp, float seek, PlaybackPosType type)
        {
            _wallpapers.ForEach(x =>
            {
                if (x.Metadata == wp)
                {
                    x.SetPlaybackPos(seek, type);
                }
            });
        }

        public void SeekWallpaper(IMonitor display, float seek, PlaybackPosType type)
        {
            _wallpapers.ForEach(x =>
            {
                if (x.Monitor.Equals(display))
                    x.SetPlaybackPos(seek, type);
            });
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    MonitorHelper.MonitorUpdated -= MonitorSettingsChanged_Hwnd;
                    WallpaperChanged -= SetupDesktop_WallpaperChanged;
                    if (_isInitialized)
                    {
                        try
                        {
                            CloseAllWallpapers(false, true);
                            DesktopUtil.RefreshDesktop(Program.OriginalDesktopWallpaperPath);
                        }
                        catch (Exception e)
                        {

                        }
                    }
                }

                _disposed = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~WinDesktopCore()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #region helpers

        /// <summary>
        /// Adds the wp as child of spawned desktop-workerw window.
        /// </summary>
        /// <param name="windowHandle">handle of window</param>
        private void SetParentWorkerW(IntPtr windowHandle)
        {
            //Legacy, Windows 7
            if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 1)
            {
                if (!_workerw.Equals(_progman)) //this should fix the win7 wallpaper disappearing issue.
                    Win32.ShowWindow(_workerw, (uint)0);

                IntPtr ret = Win32.SetParent(windowHandle, _progman);
                if (ret.Equals(IntPtr.Zero))
                {

                    throw new Exception("Failed to set window parent.");
                }

                _workerw = _progman;
            }
            else
            {
                IntPtr ret = Win32.SetParent(windowHandle, _workerw);
                if (ret.Equals(IntPtr.Zero))
                {
                    throw new Exception("Failed to set window parent.");
                }
            }
        }

        public void SendMessage(IWallpaperMetadata metadata, IPCMessage message)
        {
            throw new NotImplementedException();
        }

        public void SendMessage(IMonitor monitor, IWallpaperMetadata metadata, IPCMessage message)
        {
            throw new NotImplementedException();
        }

        #endregion // helpers
    }
}
