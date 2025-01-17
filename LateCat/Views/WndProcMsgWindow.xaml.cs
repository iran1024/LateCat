﻿using LateCat.PoseidonEngine.Abstractions;
using LateCat.PoseidonEngine.Core;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace LateCat
{
    public partial class WndProcMsgWindow : Window
    {
        private int prevExplorerPid = GetTaskbarExplorerPid();
        private DateTime prevCrashTime = DateTime.MinValue;

        public WndProcMsgWindow()
        {
            InitializeComponent();
            //Starting a hidden window outside Monitor region, rawinput receives msg through WndProc
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = -99999;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == Win32.WM_SHOWLATECAT)
            {
                Program.ShowMainWindow();
            }
            else if (msg == Win32.WM_TASKBARCREATED)
            {                
                var newExplorerPid = GetTaskbarExplorerPid();
                if (prevExplorerPid != newExplorerPid)
                {
                    var desktopCore = App.Services.GetRequiredService<IDesktopCore>();

                    if ((DateTime.Now - prevCrashTime).TotalSeconds > 30)
                    {
                        desktopCore.ResetWallpaper();
                    }
                    else
                    {
                        _ = Task.Run(() => MessageBox.Show("Windows资源管理器在过去30秒内重新启动了两次！这可能是LateCat与其他桌面自定义软件或Windows之间的冲突。为避免进一步的问题，所有壁纸均已终止。",
                                $"{Properties.Resources.TitleAppName} - {Properties.Resources.TextError}",
                                MessageBoxButton.OK, MessageBoxImage.Error));
                        desktopCore.CloseAllWallpapers();
                        desktopCore.ResetWallpaper();
                    }
                    prevCrashTime = DateTime.Now;
                    prevExplorerPid = newExplorerPid;
                }
            }
            else if (msg == (uint)Win32.WM.QUERYENDSESSION)
            {
                _ = Win32.RegisterApplicationRestart(
                    null,
                    (int)Win32.RestartFlags.RESTART_NO_CRASH |
                    (int)Win32.RestartFlags.RESTART_NO_HANG |
                    (int)Win32.RestartFlags.RESTART_NO_REBOOT);
            }

            _ = MonitorManager.Instance?.OnWndProc(hwnd, (uint)msg, wParam, lParam);

            return IntPtr.Zero;
        }

        #region helpers

        private static int GetTaskbarExplorerPid()
        {
            _ = Win32.GetWindowThreadProcessId(Win32.FindWindow("Shell_TrayWnd", null), out int pid);
            return pid;
        }

        #endregion //helpers
    }
}
