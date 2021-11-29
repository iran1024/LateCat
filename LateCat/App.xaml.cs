using LateCat.Core;
using LateCat.Factory;
using LateCat.PoseidonEngine;
using LateCat.PoseidonEngine.Abstractions;
using LateCat.PoseidonEngine.Core;
using LateCat.Services;
using LateCat.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Windows;
using System.Windows.Navigation;

namespace LateCat
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public static IServiceProvider Services
        {
            get
            {
                return ((App)Current)._serviceProvider ?? throw new InvalidOperationException("尚未初始化服务提供程序");
            }
        }

        public App()
        {
            _serviceProvider = ConfigureServices();

            if (OperatingSystem.IsWindowsVersionAtLeast(7))
            {
                CrossThreadAccessor.Initialize();
            }

            else
            {
                MessageBox.Show("仅支持Windows 7及更高版本的Windows操作系统");
                Program.ExitApplication();
            }
        }

        private static IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<MainWindow>()
                .AddSingleton<ISettingsService, SettingsService>()
                .AddSingleton<IDesktopCore, DesktopCore>()
                .AddSingleton<IWatchdogService, WatchdogService>()
                .AddSingleton<IPlayback, Playback>()
                .AddSingleton<ITrayIcon, TrayIcon>()
                .AddSingleton<ITaskbarOperator, TaskbarOperator>()
                .AddSingleton<SettingsViewModel>()
                .AddSingleton<WallpaperListViewModel>()
                .AddSingleton<RawInputDX>()
                .AddSingleton<WndProcMsgWindow>()

                .AddTransient<MonitorLayoutViewModel>()
                .AddTransient<IWallpaperFactory, WallpaperFactory>()
                .AddTransient<IPropertyFactory, PropertyFactory>()

                .BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                Directory.CreateDirectory(Constants.Paths.AppDataDir);
                Directory.CreateDirectory(Constants.Paths.TempDir);
                Directory.CreateDirectory(Constants.Paths.TempCefDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "AppData Directory Initialize Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Program.ExitApplication();
            }

            FileOperations.EmptyDirectory(Constants.Paths.TempDir);

            MonitorHelper.Initialize();

            var settings = Services.GetRequiredService<ISettingsService>();
            Program.WallpaperDir = settings.Settings.WallpaperDir;
            Program.WallpaperTempDir = settings.Settings.WallpaperTempDir;
            Program.WallpaperDataDir = settings.Settings.WallpaperDataDir;

            try
            {
                CreateWallpaperDir();
            }
            catch
            {
                Program.WallpaperDir = settings.Settings.WallpaperDir = Constants.Paths.WallpaperDir;
                Program.WallpaperTempDir = settings.Settings.WallpaperTempDir = Constants.Paths.WallpaperTempDir;
                Program.WallpaperDataDir = settings.Settings.WallpaperDataDir = Constants.Paths.WallpaperDataDir;

                settings.SaveSettings();

                try
                {
                    CreateWallpaperDir();
                }
                catch (Exception ie)
                {
                    MessageBox.Show(ie.Message, "Error: Failed to create wallpaper folder", MessageBoxButton.OK, MessageBoxImage.Error);
                    Program.ExitApplication();
                }
            }

            var mainWnd = Services.GetRequiredService<MainWindow>();
            Current.MainWindow = mainWnd;

            Services.GetRequiredService<WndProcMsgWindow>().Show();
            Services.GetRequiredService<RawInputDX>().Show();
            Services.GetRequiredService<IDesktopCore>().RestoreWallpaper();

            base.OnStartup(e);
        }

        protected override void OnLoadCompleted(NavigationEventArgs e)
        {
            Current.MainWindow.Show();
        }

        private static void CreateWallpaperDir()
        {
            Directory.CreateDirectory(Program.WallpaperDir);
            Directory.CreateDirectory(Program.WallpaperTempDir);
            Directory.CreateDirectory(Program.WallpaperDataDir);
        }
    }
}