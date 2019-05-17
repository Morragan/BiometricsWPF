using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Biometria.Extensions
{
    public static class ThinningExtensions
    {
        #region KMM tables
        private static readonly int[] deletionArray = new int[] { 3, 5, 7, 12, 13, 14, 15, 20, 21, 22, 23, 28, 29, 30, 31, 48, 52, 53, 54, 55, 56, 60, 61, 62, 63, 65, 67, 69, 71, 77, 79, 80, 81, 83, 84, 85, 86, 87, 88, 89, 91, 92, 93, 94, 95, 97, 99, 101, 103, 109, 111, 112, 113, 115, 116, 117, 118, 119, 120, 121, 123, 124, 125, 126, 127, 131, 133, 135, 141, 143, 149, 151, 157, 159, 181, 183, 189, 191, 192, 193, 195, 197, 199, 205, 207, 208, 209, 211, 212, 213, 214, 215, 216, 217, 219, 220, 221, 222, 223, 224, 225, 227, 229, 231, 237, 239, 240, 241, 243, 244, 245, 246, 247, 248, 249, 251, 252, 253, 254, 255 },
            pixelWeightsToMarkAs3 = new int[] { 127, 223, 247, 253 },
            pixelWeightsToMarkAs4 = new int[] { 3, 6, 12, 24, 48, 96, 192, 129, 7, 14, 28, 56, 112, 224, 193, 131, 15, 30, 60, 120, 240, 225, 195, 135 };
        #endregion
        #region K3M tables
        private static readonly int[] A0 = new int[] { 3, 6, 7, 12, 14, 15, 24, 28, 30, 31, 48, 56, 60, 62, 63, 96, 112, 120, 124, 126, 127, 129, 131, 135, 143, 159, 191, 192, 193, 195, 199, 207, 223, 224, 225, 227, 231, 239, 240, 241, 243, 247, 248, 249, 251, 252, 253, 254 },
            A1 = new int[] { 7, 14, 28, 56, 112, 131, 193, 224 },
            A2 = new int[] { 7, 14, 15, 28, 30, 56, 60, 112, 120, 131, 135, 193, 195, 224, 225, 240 },
            A3 = new int[] { 7, 14, 15, 28, 30, 31, 56, 60, 62, 112, 120, 124, 131, 135, 143, 193, 195, 199, 224, 225, 227, 240, 241, 248 },
            A4 = new int[] { 7, 14, 15, 28, 30, 31, 56, 60, 62, 63, 112, 120, 124, 126, 131, 135, 143, 159, 193, 195, 199, 207, 224, 225, 227, 231, 240, 241, 243, 248, 249, 252 },
            A5 = new int[] { 7, 14, 15, 28, 30, 31, 56, 60, 62, 63, 112, 120, 124, 126, 131, 135, 143, 159, 191, 193, 195, 199, 207, 224, 225, 227, 231, 239, 240, 241, 243, 248, 249, 251, 252, 254 },
            A1pix = new int[] { 3, 6, 7, 12, 14, 15, 24, 28, 30, 31, 48, 56, 60, 62, 63, 96, 112, 120, 124, 126, 127, 129, 131, 135, 143, 159, 191, 192, 193, 195, 199, 207, 223, 224, 225, 227, 231, 239, 240, 241, 243, 247, 248, 249, 251, 252, 253, 254 };
        #endregion
        /// <summary>
        /// Performs thinning on the bitmap, using KMM method
        /// </summary>
        /// <param name="bitmapSource">Binarized bitmap</param>
        /// <returns></returns>
        public static BitmapSource KMMThinning(this BitmapSource bitmapSource)
        {
            var writeableBitmap = new WriteableBitmap(bitmapSource);
            int stride = writeableBitmap.BackBufferStride;
            int arraySize = stride * bitmapSource.PixelHeight;
            var pixels = new byte[arraySize];
            var map = new int[bitmapSource.PixelWidth, bitmapSource.PixelHeight];
            writeableBitmap.ForEach((x, y, color) =>
            {
                if (color.R == 0) map[x, y] = 1;
                else map[x, y] = 0;
                return color;
            });

            int thinnedPixelsCount = 1;
            do thinnedPixelsCount = KMMStep(map);
            while (thinnedPixelsCount != 0);
            writeableBitmap.ForEach((x, y, color) =>
            {
                if (map[x, y] != 0) return Colors.Black;
                else return Colors.White;
            });
            return writeableBitmap.ToBitmapSource();
        }
        /// <summary>
        /// Performs a step of KMM thinning algorithm
        /// </summary>
        /// <param name="map"></param>
        /// <returns>Number of pixels thinned. If the method returns 0, the algorithm should be finished</returns>
        private static int KMMStep(int[,] map)
        {
            int pixelsThinned = 0,
                mapWidth = map.GetLength(0),
                mapHeight = map.GetLength(1);
            for (int x = 0; x < mapWidth; x++)
                for (int y = 0; y < mapHeight; y++)
                {
                    if (map[x, y] == 0) continue;
                    int pixelWeight = PixelWeight(map, x, y);
                    if (pixelWeightsToMarkAs4.Contains(pixelWeight))
                        map[x, y] = 4;
                    else if (pixelWeightsToMarkAs3.Contains(pixelWeight))
                        map[x, y] = 3;
                    else if (pixelWeight != 255)
                        map[x, y] = 2;
                }

            for (int y = 0; y < mapHeight; y++)
                for (int x = 0; x < mapWidth; x++)
                {
                    if (map[x, y] != 4) continue;
                    if (deletionArray.Contains(PixelWeight(map, x, y)))
                    {
                        map[x, y] = 0;
                        pixelsThinned++;
                    }
                    else
                        map[x, y] = 1;
                }

            for (int y = 0; y < mapHeight; y++)
                for (int x = 0; x < mapWidth; x++)
                {
                    if (map[x, y] != 2) continue;
                    if (deletionArray.Contains(PixelWeight(map, x, y)))
                    {
                        map[x, y] = 0;
                        pixelsThinned++;
                    }
                    else
                        map[x, y] = 1;
                }

            for (int y = 0; y < mapHeight; y++)
                for (int x = 0; x < mapWidth; x++)
                {
                    if (map[x, y] != 3) continue;
                    if (deletionArray.Contains(PixelWeight(map, x, y)))
                    {
                        map[x, y] = 0;
                        pixelsThinned++;
                    }
                    else
                        map[x, y] = 1;
                }
            return pixelsThinned;
        }

        public static BitmapSource K3MThinning(this BitmapSource bitmapSource)
        {
            var writeableBitmap = new WriteableBitmap(bitmapSource);
            int stride = writeableBitmap.BackBufferStride;
            int arraySize = stride * bitmapSource.PixelHeight;
            var pixels = new byte[arraySize];
            var map = new int[bitmapSource.PixelWidth, bitmapSource.PixelHeight];
            writeableBitmap.ForEach((x, y, color) =>
            {
                if (color.R == 0) map[x, y] = 1;
                else map[x, y] = 0;
                return color;
            });

            int thinnedPixelsCount = 1;
            do thinnedPixelsCount = K3MStep(map);
            while (thinnedPixelsCount != 0);

            for (int y = 0; y < writeableBitmap.PixelHeight; y++)
                for (int x = 0; x < writeableBitmap.PixelWidth; x++)
                {
                    if (deletionArray.Contains(PixelWeight(map, x, y)))
                        map[x, y] = 0;
                }

            writeableBitmap.ForEach((x, y, color) =>
            {
                if (map[x, y] != 0) return Colors.Black;
                else return Colors.White;
            });

            return writeableBitmap.ToBitmapSource();
        }

        private static int K3MStep(int[,] map)
        {
            int pixelsThinned = 0,
                   mapWidth = map.GetLength(0),
                   mapHeight = map.GetLength(1);
            List<(int x, int y)> border = new List<(int x, int y)>();
            for (int x = 0; x < mapWidth; x++)
                for (int y = 0; y < mapHeight; y++)
                {
                    if (A0.Contains(PixelWeight(map, x, y)))
                        border.Add((x, y));
                }
            foreach (var (x, y) in border)
                if (map[x, y] != 0 && A1.Contains(PixelWeight(map, x, y)))
                {
                    map[x, y] = 0;
                    pixelsThinned++;
                }
            foreach (var (x, y) in border)
                if (map[x, y] != 0 && A2.Contains(PixelWeight(map, x, y)))
                {
                    map[x, y] = 0;
                    pixelsThinned++;
                }
            foreach (var (x, y) in border)
                if (map[x, y] != 0 && A3.Contains(PixelWeight(map, x, y)))
                {
                    map[x, y] = 0;
                    pixelsThinned++;
                }
            foreach (var (x, y) in border)
                if (map[x, y] != 0 && A4.Contains(PixelWeight(map, x, y)))
                {
                    map[x, y] = 0;
                    pixelsThinned++;
                }
            foreach (var (x, y) in border)
                if (map[x, y] != 0 && A5.Contains(PixelWeight(map, x, y)))
                {
                    map[x, y] = 0;
                    pixelsThinned++;
                }

            return pixelsThinned;
        }
        /// <summary>
        /// Calculates weight of a pixel, using neighbourhood bit values matrix
        /// </summary>
        /// <param name="map"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private static int PixelWeight(int[,] map, int x, int y)
        {
            Func<bool, int> bti = Convert.ToInt32;
            if (x == 0 && y == 0)
                return bti(map[x, y + 1] != 0) * 16 + bti(map[x + 1, y + 1] != 0) * 8 + bti(map[x + 1, y] != 0) * 4;
            else if (x == 0 && y == map.GetLength(1) - 1)
                return bti(map[x + 1, y] != 0) * 4 + bti(map[x + 1, y - 1] != 0) * 2 + bti(map[x, y - 1] != 0);
            else if (x == 0)
                return bti(map[x, y + 1] != 0) * 16 + bti(map[x + 1, y + 1] != 0) * 8 + bti(map[x + 1, y] != 0) * 4 + bti(map[x + 1, y - 1] != 0) * 2 + bti(map[x, y - 1] != 0);
            else if (x == map.GetLength(0) - 1 && y == 0)
                return bti(map[x, y + 1] != 0) * 16 + bti(map[x - 1, y] != 0) * 64 + bti(map[x - 1, y + 1] != 0) * 32;
            else if (x == map.GetLength(0) - 1 && y == map.GetLength(1) - 1)
                return bti(map[x, y - 1] != 0) + bti(map[x - 1, y - 1] != 0) * 128 + bti(map[x - 1, y] != 0) * 64;
            else if (x == map.GetLength(0) - 1)
                return bti(map[x, y + 1] != 0) * 16 + bti(map[x, y - 1] != 0) + bti(map[x - 1, y - 1] != 0) * 128 + bti(map[x - 1, y] != 0) * 64 + bti(map[x - 1, y + 1] != 0) * 32;
            else if (y == 0)
                return bti(map[x, y + 1] != 0) * 16 + bti(map[x + 1, y + 1] != 0) * 8 + bti(map[x + 1, y] != 0) * 4 + bti(map[x - 1, y] != 0) * 64 + bti(map[x - 1, y + 1] != 0) * 32;
            else if (y == map.GetLength(1) - 1)
                return bti(map[x + 1, y] != 0) * 4 + bti(map[x + 1, y - 1] != 0) * 2 + bti(map[x, y - 1] != 0) + bti(map[x - 1, y - 1] != 0) * 128 + bti(map[x - 1, y] != 0) * 64;
            else
                return bti(map[x, y + 1] != 0) * 16 + bti(map[x + 1, y + 1] != 0) * 8 + bti(map[x + 1, y] != 0) * 4 + bti(map[x + 1, y - 1] != 0) * 2 + bti(map[x, y - 1] != 0) + bti(map[x - 1, y - 1] != 0) * 128 + bti(map[x - 1, y] != 0) * 64 + bti(map[x - 1, y + 1] != 0) * 32;
        }

        public static List<(int x, int y)> GetMinutiae(this BitmapSource bitmapSource)
        {
            var minutiae = new List<(int x, int y)>();
            var writeableBitmap = new WriteableBitmap(bitmapSource);
            int width = bitmapSource.PixelWidth,
                height = bitmapSource.PixelHeight;

            var map = new int[width, height];
            writeableBitmap.ForEach((x, y, color) =>
            {
                if (color.R == 0) map[x, y] = 1;
                else map[x, y] = 0;
                return color;
            });

            var crop = GetCrop(map);
            for (int x = crop.croppedXStart + 10; x < width - crop.croppedXEnd - 9; x++)
                for (int y = crop.croppedYStart + 10; y < height - crop.croppedYEnd - 9; y++)
                {
                    if (map[x, y] == 0) continue;
                    int weight = PixelWeight(map, x, y);
                    var p = new int[8];
                    for (int i = 7; i >= 0; i--)
                    {
                        int pow = (int)Math.Pow(2, i);
                        p[7 - i] = weight / pow;
                        weight %= pow;
                    }
                    int CN = (int)(0.5 * (Math.Abs(p[5] - p[6]) + Math.Abs(p[6] - p[7]) + Math.Abs(p[7] - p[0]) + Math.Abs(p[0] - p[1])
                    + Math.Abs(p[1] - p[2]) + Math.Abs(p[2] - p[3]) + Math.Abs(p[3] - p[4]) + Math.Abs(p[4] - p[5])));
                    if (CN == 0 || CN == 1 || CN == 3 || CN == 4)
                        minutiae.Add((x, y));
                }

            var minutiaeToDelete = new List<(int x, int y)>();
            for (int i = 0; i < minutiae.Count; i++)
                for (int j = i + 1; j < minutiae.Count; j++)
                {
                    double distance = Math.Sqrt((minutiae[i].x - minutiae[j].x) * (minutiae[i].x - minutiae[j].x) + (minutiae[i].y - minutiae[j].y) * (minutiae[i].y - minutiae[j].y));
                    if (distance < 6)
                    {
                        minutiaeToDelete.Add(minutiae[i]);
                        minutiaeToDelete.Add(minutiae[j]);
                    }
                }
            foreach (var item in minutiaeToDelete)
                minutiae.Remove(item);

            return minutiae;
        }
        private static (int croppedXStart, int croppedXEnd, int croppedYStart, int croppedYEnd) GetCrop(int[,] map)
        {
            int minX = 0, maxX = 0, minY = 0, maxY = 0;
            int width = map.GetLength(0), height = map.GetLength(1);
            bool stop = false;
            for (int x = 0; x < width && !stop; x++)
                for (int y = 0; y < height; y++)
                {
                    if (map[x, y] != 0)
                    {
                        minX = x;
                        stop = true;
                        break;
                    }
                }
            stop = false;

            for (int x = width - 1; x >= 0 && !stop; x--)
                for (int y = 0; y < height; y++)
                {
                    if (map[x, y] != 0)
                    {
                        maxX = x;
                        stop = true;
                        break;
                    }
                }
            stop = false;

            for (int y = 0; y < height && !stop; y++)
                for (int x = 0; x < width; x++)
                {
                    if (map[x, y] != 0)
                    {
                        minY = y;
                        stop = true;
                        break;
                    }
                }
            stop = false;

            for (int y = height - 1; y >= 0 && !stop; y--)
                for (int x = 0; x < width; x++)
                {
                    if (map[x, y] != 0)
                    {
                        maxY = y;
                        stop = true;
                        break;
                    }
                }
            return (minX, width - maxX, minY, height - maxY);
        }
    }
}
