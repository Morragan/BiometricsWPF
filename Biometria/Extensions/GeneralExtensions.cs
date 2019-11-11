using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Biometria.Extensions
{
    public static class GeneralExtensions
    {
        public static double Variance(this IEnumerable<int> numbers)
        {
            double average = numbers.Average();
            return numbers.Select(val => (val - average) * (val - average)).Sum() / numbers.Count();
        }

        public static double Covariance(this IEnumerable<double> vector1, IEnumerable<double> vector2)
        {
            if (vector1.Count() != vector2.Count()) throw new ArgumentException("Vectors must be of the same length");
            double average1 = vector1.Average();
            double average2 = vector2.Average();
            double sum = 0;

            int n = vector1.Count();
            for (int i = 0; i < n; i++)
                sum += (vector1.ElementAt(i) - average1) * (vector2.ElementAt(i) - average2);

            return sum / (n - 1);
        }

        public static double[] Mean(this IEnumerable<IEnumerable<double>> vectors)
        {
            int length = vectors.First().Count();
            var outputVector = new double[length];
            for (int i = 0; i < length; i++)
                outputVector[i] = vectors.Select(vector => vector.ElementAt(i)).Sum() / length;
            return outputVector;
        }

        public static (byte R, byte G, byte B) GetRGB(this byte[] pixels, int x, int y, int stride)
        {
            return (
                pixels[y * stride + 4 * x + 2],     // R
                pixels[y * stride + 4 * x + 1],     // G
                pixels[y * stride + 4 * x]          // B
                );
        }

        public static double[,] Add(this double[,] matrix1, double[,] matrix2)
        {
            int width = matrix1.GetLength(0),
                height = matrix1.GetLength(1);
            var output = new double[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    output[x, y] = matrix1[x, y] + matrix2[x, y];
            return output;
        }

        public static void SetRGB(this byte[] pixels, int x, int y, byte R, byte G, byte B, int stride)
        {
            pixels[y * stride + 4 * x + 2] = R;
            pixels[y * stride + 4 * x + 1] = G;
            pixels[y * stride + 4 * x] = B;
        }

        public static int GetIndex(this WriteableBitmap writeableBitmap, int x, int y) =>
            (int)(y * writeableBitmap.PixelHeight / writeableBitmap.Height) * writeableBitmap.BackBufferStride + 4 * (int)(x * writeableBitmap.PixelWidth / writeableBitmap.Width);

        /// <summary>
        /// Zamienia bitmapę na bitmapę w skali szarości
        /// </summary>
        /// <param name="bitmapSource"></param>
        /// <returns></returns>
        public static BitmapSource Grayscale(this BitmapSource bitmapSource)
        {
            var writeableBitmap = new WriteableBitmap(bitmapSource);
            return writeableBitmap.Gray();
        }

        public static BitmapSource GrayscaleBetter(this BitmapSource bitmapSource)
        {
            var writeableBitmap = new WriteableBitmap(bitmapSource);
            writeableBitmap.ForEachAsync((x, y, color) =>
            {
                byte newColor = (byte)(0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B).Clamp(0, 255);
                return Color.FromRgb(newColor, newColor, newColor);
            });
            return writeableBitmap.ToBitmapSource();
        }
        public static WriteableBitmap UpdateWithPixelArray(this WriteableBitmap writeableBitmap, byte[] pixels)
        {
            Int32Rect rect = new Int32Rect(0, 0, writeableBitmap.PixelWidth, writeableBitmap.PixelHeight);
            writeableBitmap.WritePixels(rect, pixels, writeableBitmap.BackBufferStride, 0);
            return writeableBitmap;
        }

        public static unsafe void ForEachAsync(this WriteableBitmap writeableBitmap, Func<int, int, Color, Color> func)
        {
            using (var context = writeableBitmap.GetBitmapContext())
            {
                var pixels = context.Pixels;
                var w = context.Width;
                var h = context.Height;

                Parallel.For(0, h, y =>
                {
                    Parallel.For(0, w, x =>
                    {
                        var c = pixels[y * w + x];

                        // Premultiplied Alpha!
                        var a = (byte)(c >> 24);
                        // Prevent division by zero
                        int ai = a;
                        if (ai == 0)
                        {
                            ai = 1;
                        }
                        // Scale inverse alpha to use cheap integer mul bit shift
                        ai = (255 << 8) / ai;
                        var srcColor = Color.FromArgb(a,
                                                      (byte)((((c >> 16) & 0xFF) * ai) >> 8),
                                                      (byte)((((c >> 8) & 0xFF) * ai) >> 8),
                                                      (byte)((((c & 0xFF) * ai) >> 8)));

                        var color = func(x, y, srcColor);
                        pixels[y * w + x] = ConvertColor(color);
                    });
                });
            }
        }
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

        private static int ConvertColor(Color color)
        {
            var col = 0;

            if (color.A != 0)
            {
                var a = color.A + 1;
                col = (color.A << 24)
                  | ((byte)((color.R * a) >> 8) << 16)
                  | ((byte)((color.G * a) >> 8) << 8)
                  | ((byte)((color.B * a) >> 8));
            }

            return col;
        }

        public static double GetHue(this Color color)
        {
            float min = Math.Min(Math.Min(color.R, color.G), color.B);
            float max = Math.Max(Math.Max(color.R, color.G), color.B);

            if (min == max) return 0;

            float hue;
            if (max == color.R)
                hue = (color.G - color.B) / (max - min);
            else if (max == color.G)
                hue = 2f + (color.B - color.R) / (max - min);
            else
                hue = 4f + (color.R - color.G) / (max - min);

            hue *= 60;
            if (hue < 0) hue += 360;

            return Math.Round(hue);
        }

        public static void ToHSV(this Color color, out double hue, out double saturation, out double value)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = color.GetHue();
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }
        public static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value *= 255;
            byte v = Convert.ToByte(value);
            byte p = Convert.ToByte(value * (1 - saturation));
            byte q = Convert.ToByte(value * (1 - f * saturation));
            byte t = Convert.ToByte(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromRgb(v, t, p);
            else if (hi == 1)
                return Color.FromRgb(q, v, p);
            else if (hi == 2)
                return Color.FromRgb(p, v, t);
            else if (hi == 3)
                return Color.FromRgb(p, q, v);
            else if (hi == 4)
                return Color.FromRgb(t, p, v);
            else
                return Color.FromRgb(v, p, q);
        }
    }
}
