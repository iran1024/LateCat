using System;
using System.Drawing;
using System.Drawing.Imaging;

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

            unsafe
            {
                var p = (byte*)(void*)scan0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        for (int delta = 0; delta < 3; delta++)
                        {
                            var idx = (y * stride) + x * 4 + delta;

                            totals[delta] += p[idx];
                        }
                    }
                }
            }

            bmp.UnlockBits(bmpData);
            GC.Collect();

            var pix = width * height;

            var r = (int)totals[2] / pix;
            var g = (int)totals[1] / pix;
            var b = (int)totals[0] / pix;

            return Color.FromArgb(r, g, b);
        }
    }
}
