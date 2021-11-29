using ImageMagick;
using LateCat.PoseidonEngine.Core;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

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

        public static async Task CaptureGif(string savePath, int x, int y, int width, int height,
            int captureDelay, int animeDelay, int totalFrames, IProgress<int> progress)
        {
            await Task.Run(async () =>
            {
                var miArray = new MagickImage[totalFrames];
                try
                {
                    for (int i = 0; i < totalFrames; i++)
                    {
                        using (var bmp = CopyScreen(x, y, width, height))
                        {
                            miArray[i] = ToMagickImage(bmp);
                        }
                        await Task.Delay(captureDelay);
                        progress.Report((i + 1) * 100 / totalFrames);
                    }

                    using MagickImageCollection collection = new MagickImageCollection();
                    for (int i = 0; i < totalFrames; i++)
                    {
                        collection.Add(miArray[i]);
                        collection[i].AnimationDelay = animeDelay;
                    }

                    var settings = new QuantizeSettings
                    {
                        Colors = 256,
                    };
                    collection.Quantize(settings);

                    collection.Optimize();

                    collection.Write(savePath);
                }
                finally
                {
                    for (int i = 0; i < totalFrames; i++)
                    {
                        miArray[i]?.Dispose();
                    }
                }
            });
        }

        #region helpers

        public static MagickImage ToMagickImage(Bitmap bmp)
        {
            var mi = new MagickImage();
            using (var ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Bmp);
                ms.Position = 0;
                mi.Read(ms);
            }
            return mi;
        }

        public static void ResizeAnimatedGif(string srcFile, string destFile, int width, int height)
        {
            using var collection = new MagickImageCollection(srcFile);

            collection.Coalesce();

            foreach (var image in collection)
            {
                image.Resize(width, height);
            }

            collection.Write(destFile);
        }

        #endregion //helpers
    }
}
