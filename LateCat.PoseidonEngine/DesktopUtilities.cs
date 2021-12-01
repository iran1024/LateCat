using System;

namespace LateCat.PoseidonEngine.Core
{
    public static class DesktopUtilities
    {
        public static bool DesktopIconVisibilityDefault { get; }

        static DesktopUtilities()
        {
            DesktopIconVisibilityDefault = GetDesktopIconVisibility();
        }

        public static bool GetDesktopIconVisibility()
        {
            var state = new Win32.SHELLSTATE();
            Win32.SHGetSetSettings(ref state, Win32.SSF.SSF_HIDEICONS, false);

            return !state.fHideIcons;
        }

        public static void SetDesktopIconVisibility(bool isVisible)
        {
            if (GetDesktopIconVisibility() ^ isVisible)
            {
                _ = Win32.SendMessage(GetDesktopSHELLDLL_DefView(), (int)Win32.WM.COMMAND, (IntPtr)0x7402, IntPtr.Zero);
            }
        }

        private static IntPtr GetDesktopSHELLDLL_DefView()
        {
            var hShellViewWin = IntPtr.Zero;
            var hWorkerW = IntPtr.Zero;

            var hProgman = Win32.FindWindow("Progman", "Program Manager");
            var hDesktopWnd = Win32.GetDesktopWindow();

            if (hProgman != IntPtr.Zero)
            {
                hShellViewWin = Win32.FindWindowEx(hProgman, IntPtr.Zero, "SHELLDLL_DefView", string.Empty);
                if (hShellViewWin == IntPtr.Zero)
                {
                    do
                    {
                        hWorkerW = Win32.FindWindowEx(hDesktopWnd, hWorkerW, "WorkerW", string.Empty);
                        hShellViewWin = Win32.FindWindowEx(hWorkerW, IntPtr.Zero, "SHELLDLL_DefView", string.Empty);

                    } while (hShellViewWin == IntPtr.Zero && hWorkerW != IntPtr.Zero);
                }
            }

            return hShellViewWin;
        }

        public static void RefreshDesktop()
        {
#pragma warning disable CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
            _ = Win32.SystemParametersInfo(Win32.SPI_SETDESKWALLPAPER, 0, null, Win32.SPIF_UPDATEINIFILE);
#pragma warning restore CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
        }
    }
}
