using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Biometria.Extensions
{
    public static class HistogramExtensions
    {
        public static (int[] rHistogram, int[] gHistogram, int[] bHistogram, int[] brightnessHistogram) Histogram(this BitmapSource bitmapSource)
        {
            int[] rHistogram = new int[256],
                gHistogram = new int[256],
                bHistogram = new int[256],
                brightnessHistogram = new int[256];
            rHistogram.Initialize();
            gHistogram.Initialize();
            bHistogram.Initialize();
            brightnessHistogram.Initialize();

            var writeableBitmap = new WriteableBitmap(bitmapSource);
            writeableBitmap.ForEach((x, y, color) =>
            {
                rHistogram[color.R]++;
                gHistogram[color.G]++;
                bHistogram[color.B]++;
                brightnessHistogram[(color.R + color.G + color.B) / 3]++;
                return color;
            });
            return (rHistogram, gHistogram, bHistogram, brightnessHistogram);
        }

        public static int[] BrightnessHistogram(this BitmapSource bitmapSource)
        {
            int[] brightnessHistogram = new int[256];
            brightnessHistogram.Initialize();

            var writeableBitmap = new WriteableBitmap(bitmapSource);
            writeableBitmap.ForEach((x, y, color) =>
            {
                brightnessHistogram[(color.R + color.G + color.B) / 3]++;
                return color;
            });
            return brightnessHistogram;
        }
    }
}
