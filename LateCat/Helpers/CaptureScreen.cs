using LateCat.PoseidonEngine.Core;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;

namespace LateCat.Helpers
{
    public static class CaptureScreen
    {
        public static void CopyScreen(string savePath, int x, int y, int width, int height)
        {
            using var screenBmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using var bmpGraphics = Graphics.FromImage(screenBmp);

            bmpGraphics.CopyFromScreen(x, y, 0, 0, screenBmp.Size);
            screenBmp.Save(savePath, ImageFormat.Jpeg);
        }

        public static Bitmap CopyScreen(int x, int y, int width, int height)
        {
            var screenBmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using (var bmpGraphics = Graphics.FromImage(screenBmp))
            {
                bmpGraphics.CopyFromScreen(x, y, 0, 0, screenBmp.Size);
                return screenBmp;
            }
        }

        public static Bitmap CaptureWindow(IntPtr hWnd)
        {
            _ = Win32.GetWindowRect(hWnd, out Win32.RECT rect);
            var region = Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);

            IntPtr winDc;
            IntPtr memoryDc;
            IntPtr bitmap;
            IntPtr oldBitmap;
            bool success;
            Bitmap result;

            winDc = Win32.GetWindowDC(hWnd);
            memoryDc = Win32.CreateCompatibleDC(winDc);
            bitmap = Win32.CreateCompatibleBitmap(winDc, region.Width, region.Height);
            oldBitmap = Win32.SelectObject(memoryDc, bitmap);

            success = Win32.BitBlt(memoryDc, 0, 0, region.Width, region.Height, winDc, region.Left, region.Top,
                Win32.TernaryRasterOperations.SRCCOPY | Win32.TernaryRasterOperations.CAPTUREBLT);

            try
            {
                if (!success)
                {
                    throw new Win32Exception();
                }

                result = Image.FromHbitmap(bitmap);
            }
            finally
            {
                Win32.SelectObject(memoryDc, oldBitmap);
                Win32.DeleteObject(bitmap);
                Win32.DeleteDC(memoryDc);
                Win32.ReleaseDC(hWnd, winDc);
            }
            return result;
        }
    }
}
