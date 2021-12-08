using LateCat.Core;
using LateCat.Helpers;
using LateCat.PoseidonEngine;
using LateCat.PoseidonEngine.Core;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace LateCat
{
    public class Program
    {
        #region init        
        private static readonly Mutex mutex = new(false, Constants.SingleInstance.UniqueAppName);

        public static string WallpaperDir { get; set; }
        public static string WallpaperTempDir { get; set; }
        public static string WallpaperDataDir { get; set; }

        public static double PreviewerWidth { get; set; }
        public static double PreviewerHeight { get; set; }
        #endregion //init

        #region app entry

        [STAThread]
        public static void Main()
        {
            try
            {
                if (!mutex.WaitOne(TimeSpan.FromSeconds(1), false))
                {
                    try
                    {
                        var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
                        PipeClient.SendMessage(Constants.SingleInstance.PipeServerName, args.Length != 0 ? args : new string[] { "--showapp", "true" });
                    }
                    catch
                    {
                        Win32.PostMessage(
                            (IntPtr)Win32.HWND_BROADCAST,
                            Win32.WM_SHOWLATECAT,
                            IntPtr.Zero,
                            IntPtr.Zero);
                    }

                    return;
                }
            }
            catch (AbandonedMutexException e)
            {
                Debug.WriteLine(e.Message);
            }

            try
            {
                var server = new PipeServer(Constants.SingleInstance.PipeServerName);
                server.MessageReceived += PipeServer_MessageReceived;
            }
            catch (Exception e)
            {
                MessageBox.Show($"创建IPC Server失败: {e.Message}", "Late Cat");
            }

            try
            {
                var app = new App();
                app.InitializeComponent();
                app.Startup += App_Startup;
                app.SessionEnding += App_SessionEnding;
                app.Run();
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        private static void PipeServer_MessageReceived(object? sender, string[] msg)
        {
            //Cmd.CommandHandler.ParseArgs(msg);
        }

        private static void App_Startup(object sender, StartupEventArgs e)
        {
            var sysTray = App.Services.GetRequiredService<ITrayIcon>();
            sysTray.ShowBalloonNotification(5, "欢迎追踪迷失的猫", "Mooooooooo");
        }

        #endregion //app entry

        #region app sessions

        private static void App_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            if (e.ReasonSessionEnding == ReasonSessionEnding.Shutdown || e.ReasonSessionEnding == ReasonSessionEnding.Logoff)
            {
                e.Cancel = true;
                ExitApplication();
            }
        }

        public static void ShowMainWindow()
        {
            var mainWnd = App.Services.GetRequiredService<MainWindow>();

            mainWnd?.Show();
        }

        public static void ExitApplication()
        {
            MainWindow.IsExit = true;

            ((ServiceProvider)App.Services)?.Dispose();

            Application.Current.Shutdown();
        }

        #endregion //app sessions
    }
}
