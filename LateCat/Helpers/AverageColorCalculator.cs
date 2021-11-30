using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace LateCat.Helpers
{
    internal static class AverageColorCalculator
    {
        public static Color Analyze(Bitmap bmp)
        {
            var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            var stride = bmpData.Stride;

            var scan0 = bmpData.Scan0;

            var totals = new long[] { 0, 0, 0 };

            var width = bmp.Width;
            var height = bmp.Height;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int rgbIdx = 0; rgbIdx < 3; rgbIdx++)
                    {
                        totals[rgbIdx] += Marshal.ReadByte(scan0 + (y * stride) + x * 4 + rgbIdx);
                    }
                }
            }

            bmp.UnlockBits(bmpData);
            GC.Collect();

            var pix = width * height;

            return Color.FromArgb((int)totals[2] / pix, (int)totals[1] / pix, (int)totals[0] / pix);
        }
    }
}
