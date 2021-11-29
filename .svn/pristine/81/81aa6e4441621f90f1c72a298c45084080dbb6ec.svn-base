using LateCat.PoseidonEngine.Core;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace LateCat
{
    public static class WindowOperations
    {
        public static Task ShowWindowAsync(Window window)
        {
            var tcs = new TaskCompletionSource<object>();
            var thread = new Thread(() =>
            {
                try
                {
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                    {
                        window.Closed += (s, a) => tcs.SetResult(0);
                        window.Show();
                        window.Focus();
                    }));
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }

        public static Task ShowWindowDialogAsync(Window window)
        {
            var tcs = new TaskCompletionSource<object>();
            var thread = new Thread(() =>
            {
                try
                {
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                    {
                        window.ShowDialog();
                        tcs.SetResult(0);
                    }));
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }

        public static Rect GetAbsolutePlacement(FrameworkElement element, bool relativeToMonitor = false)
        {
            var absolutePos = element.PointToScreen(new Point(0, 0));
            if (relativeToMonitor)
            {
                var pixelSize = GetElementPixelSize(element);

                return new Rect(absolutePos.X, absolutePos.Y, pixelSize.Width, pixelSize.Height);
            }
            var posMW = Application.Current.MainWindow.PointToScreen(new Point(0, 0));
            absolutePos = new Point(absolutePos.X - posMW.X, absolutePos.Y - posMW.Y);
            return new Rect(absolutePos.X, absolutePos.Y, element.ActualWidth, element.ActualHeight);
        }

        public static Size GetElementPixelSize(UIElement element)
        {
            Matrix transformToDevice;
            var source = PresentationSource.FromVisual(element);
            if (source != null)
                transformToDevice = source.CompositionTarget.TransformToDevice;
            else
                using (var source1 = new HwndSource(new HwndSourceParameters()))
                    transformToDevice = source1.CompositionTarget.TransformToDevice;

            if (element.DesiredSize == new Size())
                element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            return (Size)transformToDevice.Transform((Vector)element.DesiredSize);
        }

        public static void SetProgramToFramework(Window window, IntPtr pgmHandle, FrameworkElement element)
        {
            IntPtr previewHwnd = new WindowInteropHelper(window).Handle;

            var prct = new Win32.RECT();

            var reviewPanel = WindowOperations.GetAbsolutePlacement(element, true);

            if (!Win32.SetWindowPos(pgmHandle, 1, (int)reviewPanel.Left, (int)reviewPanel.Top, (int)reviewPanel.Width, (int)reviewPanel.Height, 0 | 0x0010))
            {

            }

            _ = Win32.MapWindowPoints(pgmHandle, previewHwnd, ref prct, 2);

            SetParentSafe(pgmHandle, previewHwnd);

            if (!Win32.SetWindowPos(pgmHandle, 1, prct.Left, prct.Top, (int)reviewPanel.Width, (int)reviewPanel.Height, 0 | 0x0010))
            {

            }
        }

        public static void SetParentSafe(IntPtr child, IntPtr parent)
        {
            IntPtr ret = Win32.SetParent(child, parent);
            if (ret.Equals(IntPtr.Zero))
            {

            }
        }

        public static void BorderlessWinStyle(IntPtr handle)
        {
            var styleCurrentWindowStandard = Win32.GetWindowLongPtr(handle, (int)Win32.GWL.GWL_STYLE);
            var styleCurrentWindowExtended = Win32.GetWindowLongPtr(handle, (int)Win32.GWL.GWL_EXSTYLE);

            var styleNewWindowStandard =
                              styleCurrentWindowStandard.ToInt64()
                              & ~(
                                    Win32.WindowStyles.WS_CAPTION
                                  | Win32.WindowStyles.WS_THICKFRAME
                                  | Win32.WindowStyles.WS_SYSMENU
                                  | Win32.WindowStyles.WS_MAXIMIZEBOX
                                  | Win32.WindowStyles.WS_MINIMIZEBOX
                              );


            var styleNewWindowExtended =
                styleCurrentWindowExtended.ToInt64()
                & ~(
                      Win32.WindowStyles.WS_EX_DLGMODALFRAME
                    | Win32.WindowStyles.WS_EX_COMPOSITED
                    | Win32.WindowStyles.WS_EX_WINDOWEDGE
                    | Win32.WindowStyles.WS_EX_CLIENTEDGE
                    | Win32.WindowStyles.WS_EX_LAYERED
                    | Win32.WindowStyles.WS_EX_STATICEDGE
                    | Win32.WindowStyles.WS_EX_TOOLWINDOW
                    | Win32.WindowStyles.WS_EX_APPWINDOW
                );

            if (Win32.SetWindowLongPtr(new HandleRef(null, handle), (int)Win32.GWL.GWL_STYLE, (IntPtr)styleNewWindowStandard) == IntPtr.Zero)
            {

            }

            if (Win32.SetWindowLongPtr(new HandleRef(null, handle), (int)Win32.GWL.GWL_EXSTYLE, (IntPtr)styleNewWindowExtended) == IntPtr.Zero)
            {

            }

            var menuHandle = Win32.GetMenu(handle);
            if (menuHandle != IntPtr.Zero)
            {
                var menuItemCount = Win32.GetMenuItemCount(menuHandle);

                for (var i = 0; i < menuItemCount; i++)
                {
                    Win32.RemoveMenu(menuHandle, 0, Win32.MF_BYPOSITION | Win32.MF_REMOVE);
                }

                Win32.DrawMenuBar(handle);
            }
        }

        public static void RemoveWindowFromTaskbar(IntPtr handle)
        {
            var styleCurrentWindowExtended = Win32.GetWindowLongPtr(handle, (int)Win32.GWL.GWL_EXSTYLE);

            var styleNewWindowExtended = styleCurrentWindowExtended.ToInt64() |
                   Win32.WindowStyles.WS_EX_NOACTIVATE |
                   Win32.WindowStyles.WS_EX_TOOLWINDOW;

            Win32.ShowWindow(handle, (int)Win32.SHOWWINDOW.SW_HIDE);

            if (Win32.SetWindowLongPtr(new HandleRef(null, handle), (int)Win32.GWL.GWL_EXSTYLE, (IntPtr)styleNewWindowExtended) == IntPtr.Zero)
            {

            }

            Win32.ShowWindow(handle, (int)Win32.SHOWWINDOW.SW_SHOW);
        }

        public const int LWA_ALPHA = 0x2;
        public const int LWA_COLORKEY = 0x1;

        public static void SetWindowTransparency(IntPtr Handle)
        {
            var styleCurrentWindowExtended = Win32.GetWindowLongPtr(Handle, (-20));
            var styleNewWindowExtended =
                styleCurrentWindowExtended.ToInt64() ^
                Win32.WindowStyles.WS_EX_LAYERED;

            Win32.SetWindowLongPtr(new HandleRef(null, Handle), (int)Win32.GWL.GWL_EXSTYLE, (IntPtr)styleNewWindowExtended);
            Win32.SetLayeredWindowAttributes(Handle, 0, 128, LWA_ALPHA);
        }
    }
}
