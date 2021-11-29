using LateCat.PoseidonEngine.Abstractions;
using LateCat.PoseidonEngine.Core;
using LateCat.ViewModels;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace LateCat.Views
{
    public interface IWallpaperPreview
    {
        public void Exit();
        public void Ok();
    }

    public partial class WallpaperPreviewView : Window, IWallpaperPreview
    {
        private bool _processing = false;

        private readonly IntPtr wallpaperHwnd;

        private readonly ISettingsService _settings;        

        public WallpaperPreviewView(IWallpaper wallpaper)
        {
            DataContext = new WallpaperPreviewViewModel(this, wallpaper);
            
            Closed += ((WallpaperPreviewViewModel)DataContext).OnWindowClosed;

            wallpaperHwnd = wallpaper.Handle;

            InitializeComponent();
            PreviewKeyDown += (s, e) => { if (e.Key == Key.Escape) Close(); };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowOperations.SetProgramToFramework(this, wallpaperHwnd, PreviewBorder);

            Activate();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_processing)
            {
                e.Cancel = true;

                return;
            }

            WindowOperations.SetParentSafe(wallpaperHwnd, IntPtr.Zero);
        }

        public void Exit()
        {
            Close();
        }

        public void Ok()
        {
            ((WallpaperPreviewViewModel)DataContext).Ok = true;
            Close();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }

        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == (int)Win32.WM.WINDOWPOSCHANGING && _processing)
            {
                var wp = Marshal.PtrToStructure<Win32.WINDOWPOS>(lParam);
                wp.flags |= (int)Win32.SetWindowPosFlags.SWP_NOMOVE | (int)Win32.SetWindowPosFlags.SWP_NOSIZE;
                Marshal.StructureToPtr(wp, lParam, false);
            }
            return IntPtr.Zero;
        }
    }
}
