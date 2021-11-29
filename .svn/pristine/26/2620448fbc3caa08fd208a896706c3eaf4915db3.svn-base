using LateCat.PoseidonEngine.Abstractions;
using LateCat.PoseidonEngine.Core;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace LateCat.Core.Cef
{
    public partial class PropertiesWindow : Window
    {
        public PropertiesWindow(IWallpaperMetadata metadata)
        {
            InitializeComponent();
            PreviewKeyDown += (s, e) => { if (e.Key == Key.Escape) Close(); };
        }

        #region window move/resize lock

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var source = PresentationSource.FromVisual(this) as HwndSource;

            source?.AddHook(WndProc);
        }

        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == (int)Win32.WM.WINDOWPOSCHANGING)
            {
                var wp = Marshal.PtrToStructure<Win32.WINDOWPOS>(lParam);
                wp.flags |= (int)Win32.SetWindowPosFlags.SWP_NOMOVE | (int)Win32.SetWindowPosFlags.SWP_NOSIZE;
                Marshal.StructureToPtr(wp, lParam, false);
            }
            return IntPtr.Zero;
        }

        #endregion //window move/resize lock
    }
}
