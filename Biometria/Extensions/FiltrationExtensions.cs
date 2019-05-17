using System;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Biometria.Extensions
{
    public static class FiltrationExtentions
    {
        public static (byte[] R, byte[] G, byte[] B) GetWindow(this byte[] pixels, int filterSize, int x, int y, int stride, int bitmapWidth, int bitmapHeight)
        {
            int radius = filterSize / 2;
            if (x - radius < 0 || y - radius < 0 || x + radius >= bitmapWidth || y + radius >= bitmapHeight)
                return (new byte[0], new byte[0], new byte[0]);
            byte[] filterR = new byte[filterSize * filterSize],
                filterG = new byte[filterSize * filterSize],
                filterB = new byte[filterSize * filterSize];
            for (int _x = -radius; _x <= radius; _x++)
                for (int _y = -radius; _y <= radius; _y++) 
                {
                    var (R, G, B) = pixels.GetRGB(_x + x, _y + y, stride);
                    filterR[(_x + radius) * filterSize + _y + radius] = R;
                    filterG[(_x + radius) * filterSize + _y + radius] = G;
                    filterB[(_x + radius) * filterSize + _y + radius] = B;
                }
            return (filterR, filterG, filterB);
        }
        public static byte[] Convolute(this byte[] pixels, int[] filter, int stride, int height, int width, double hBitsPerPixel, double wBitsPerPixel)
        {
            byte[] pixelsFiltered = (byte[])pixels.Clone();

            if (filter.Length == 9)
            {
                for (int x = 0; x < width - 1; x++)
                    for (int y = 0; y < height - 1; y++)
                    {
                        if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                        {
                            pixelsFiltered.SetRGB(x, y, 0, 0, 0, stride);
                            continue;
                        }
                        byte sum = (byte)Math.Abs(filter[0] * pixels.GetRGB(x - 1, y - 1, stride).R +
                            filter[1] * pixels.GetRGB(x, y - 1, stride).R +
                            filter[2] * pixels.GetRGB(x + 1, y - 1, stride).R +
                            filter[3] * pixels.GetRGB(x - 1, y, stride).R +
                            filter[4] * pixels.GetRGB(x, y, stride).R +
                            filter[5] * pixels.GetRGB(x + 1, y, stride).R +
                            filter[6] * pixels.GetRGB(x - 1, y + 1, stride).R +
                            filter[7] * pixels.GetRGB(x, y + 1, stride).R +
                            filter[8] * pixels.GetRGB(x + 1, y + 1, stride).R);
                        pixelsFiltered.SetRGB(x, y, sum, sum, sum, stride);
                    }
            }
            else
            {
                for (int x = 0; x < width - 1; x++)
                    for (int y = 0; y < height - 1; y++)
                    {
                        if (x == 0 || x == 1 || x == width - 2 || x == width - 1 || y == 0 || y == 1 || y == height - 2 || y == height - 1)
                        {
                            pixelsFiltered.SetRGB(x, y, 0, 0, 0, stride);
                            continue;
                        }
                        byte sum = (byte)Math.Abs(filter[0] * pixels.GetRGB(x - 2, y - 2, stride).R +
                            filter[1] * pixels.GetRGB(x - 1, y - 2, stride).R +
                            filter[2] * pixels.GetRGB(x, y - 2, stride).R +
                            filter[3] * pixels.GetRGB(x + 1, y - 2, stride).R +
                            filter[4] * pixels.GetRGB(x + 2, y - 2, stride).R +
                            filter[5] * pixels.GetRGB(x - 2, y - 1, stride).R +
                            filter[6] * pixels.GetRGB(x - 1, y - 1, stride).R +
                            filter[7] * pixels.GetRGB(x, y - 1, stride).R +
                            filter[8] * pixels.GetRGB(x + 1, y - 1, stride).R +
                            filter[9] * pixels.GetRGB(x + 2, y - 1, stride).R +
                            filter[10] * pixels.GetRGB(x - 2, y, stride).R +
                            filter[11] * pixels.GetRGB(x - 1, y, stride).R +
                            filter[12] * pixels.GetRGB(x, y, stride).R +
                            filter[13] * pixels.GetRGB(x + 1, y, stride).R +
                            filter[14] * pixels.GetRGB(x + 2, y, stride).R +
                            filter[15] * pixels.GetRGB(x - 2, y + 1, stride).R +
                            filter[16] * pixels.GetRGB(x - 1, y + 1, stride).R +
                            filter[17] * pixels.GetRGB(x, y + 1, stride).R +
                            filter[18] * pixels.GetRGB(x + 1, y + 1, stride).R +
                            filter[19] * pixels.GetRGB(x + 2, y + 1, stride).R +
                            filter[20] * pixels.GetRGB(x - 2, y + 2, stride).R +
                            filter[21] * pixels.GetRGB(x - 1, y + 2, stride).R +
                            filter[22] * pixels.GetRGB(x, y + 2, stride).R +
                            filter[23] * pixels.GetRGB(x + 1, y + 2, stride).R +
                            filter[24] * pixels.GetRGB(x + 2, y + 2, stride).R);
                        pixelsFiltered.SetRGB(x, y, sum, sum, sum, stride);
                    }
            }
            return pixelsFiltered;
        }
        public static byte[] Convolute(this byte[] pixels, double[] filter, int stride, int height, int width, double hBitsPerPixel, double wBitsPerPixel)
        {
            byte[] pixelsFiltered = (byte[])pixels.Clone();

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                    {
                        pixelsFiltered.SetRGB(x, y, 0, 0, 0, stride);
                        continue;
                    }
                    byte sumR = (byte)Math.Abs(filter[0] * pixels.GetRGB(x - 1, y - 1, stride).R +
                        filter[1] * pixels.GetRGB(x, y - 1, stride).R +
                        filter[2] * pixels.GetRGB(x + 1, y - 1, stride).R +
                        filter[3] * pixels.GetRGB(x - 1, y, stride).R +
                        filter[4] * pixels.GetRGB(x, y, stride).R +
                        filter[5] * pixels.GetRGB(x + 1, y, stride).R +
                        filter[6] * pixels.GetRGB(x - 1, y + 1, stride).R +
                        filter[7] * pixels.GetRGB(x, y + 1, stride).R +
                        filter[8] * pixels.GetRGB(x + 1, y + 1, stride).R);
                    byte sumG = (byte)Math.Abs(filter[0] * pixels.GetRGB(x - 1, y - 1, stride).G +
                        filter[1] * pixels.GetRGB(x, y - 1, stride).G +
                        filter[2] * pixels.GetRGB(x + 1, y - 1, stride).G +
                        filter[3] * pixels.GetRGB(x - 1, y, stride).G +
                        filter[4] * pixels.GetRGB(x, y, stride).G +
                        filter[5] * pixels.GetRGB(x + 1, y, stride).G +
                        filter[6] * pixels.GetRGB(x - 1, y + 1, stride).G +
                        filter[7] * pixels.GetRGB(x, y + 1, stride).G +
                        filter[8] * pixels.GetRGB(x + 1, y + 1, stride).G);
                    byte sumB = (byte)Math.Abs(filter[0] * pixels.GetRGB(x - 1, y - 1, stride).B +
                        filter[1] * pixels.GetRGB(x, y - 1, stride).B +
                        filter[2] * pixels.GetRGB(x + 1, y - 1, stride).B +
                        filter[3] * pixels.GetRGB(x - 1, y, stride).B +
                        filter[4] * pixels.GetRGB(x, y, stride).B +
                        filter[5] * pixels.GetRGB(x + 1, y, stride).B +
                        filter[6] * pixels.GetRGB(x - 1, y + 1, stride).B +
                        filter[7] * pixels.GetRGB(x, y + 1, stride).B +
                        filter[8] * pixels.GetRGB(x + 1, y + 1, stride).B);
                    pixelsFiltered.SetRGB(x, y, sumR, sumG, sumB, stride);
                }

            return pixelsFiltered;
        }

        public static BitmapSource FilterCustom(this BitmapSource bitmapSource, int[] filter)
        {
            if (filter.Length != 9 && filter.Length != 25) throw new ArgumentException("Mask must be either 3x3 or 5x5");
            var writeableBitmap = new WriteableBitmap(bitmapSource);
            var pixels = new byte[writeableBitmap.PixelHeight * writeableBitmap.BackBufferStride];
            double hBitsPerPixel = writeableBitmap.PixelHeight / writeableBitmap.Height,
                wBitsPerPixel = writeableBitmap.PixelWidth / writeableBitmap.Width;

            writeableBitmap.CopyPixels(pixels, writeableBitmap.BackBufferStride, 0);
            var pixelsFiltered = pixels.Convolute(filter, writeableBitmap.BackBufferStride, (int)writeableBitmap.Height, (int)writeableBitmap.Width, hBitsPerPixel, wBitsPerPixel);
            writeableBitmap.UpdateWithPixelArray(pixelsFiltered);
            return writeableBitmap.ToBitmapSource();
        }

        public static BitmapSource Filter(this BitmapSource bitmapSource, FiltrationMethod filtrationMethod)
        {
            var writeableBitmap = new WriteableBitmap(bitmapSource);
            var pixels = new byte[writeableBitmap.PixelHeight * writeableBitmap.BackBufferStride];
            double hBitsPerPixel = writeableBitmap.PixelHeight / writeableBitmap.Height,
                wBitsPerPixel = writeableBitmap.PixelWidth / writeableBitmap.Width;
            writeableBitmap.CopyPixels(pixels, writeableBitmap.BackBufferStride, 0);
            int stride = writeableBitmap.BackBufferStride,
                width = writeableBitmap.PixelWidth,
                height = writeableBitmap.PixelHeight;

            byte[] pixelsFiltered = new byte[0];
            switch (filtrationMethod)
            {
                case FiltrationMethod.LowPass:
                    var filter = new double[] { 1.0 / 9, 1.0 / 9, 1.0 / 9, 1.0 / 9, 1.0 / 9, 1.0 / 9, 1.0 / 9, 1.0 / 9, 1.0 / 9 };
                    pixelsFiltered = pixels.Convolute(filter, writeableBitmap.BackBufferStride, (int)writeableBitmap.Height, (int)writeableBitmap.Width, hBitsPerPixel, wBitsPerPixel);
                    writeableBitmap.UpdateWithPixelArray(pixelsFiltered);
                    break;
                case FiltrationMethod.Prewitt:
                    var filter1 = new int[] { 1, 1, 1, 0, 0, 0, -1, -1, -1 };
                    var filter2 = new int[] { -1, 0, 1, -1, 0, 1, -1, 0, 1 };
                    var pixelsFilteredVertical = pixels.Convolute(filter1, writeableBitmap.BackBufferStride, (int)writeableBitmap.Height, (int)writeableBitmap.Width, hBitsPerPixel, wBitsPerPixel);
                    var pixelsFilteredHorizontal = pixels.Convolute(filter2, writeableBitmap.BackBufferStride, (int)writeableBitmap.Height, (int)writeableBitmap.Width, hBitsPerPixel, wBitsPerPixel);
                    pixelsFiltered = pixelsFilteredHorizontal.Select((x, index) => Math.Max(x, pixelsFilteredVertical[index])).ToArray();
                    writeableBitmap.UpdateWithPixelArray(pixelsFiltered);
                    break;
                case FiltrationMethod.Sobel:
                    filter1 = new int[] { 1, 2, 1, 0, 0, 0, -1, -2, -1 };
                    filter2 = new int[] { 1, 0, -1, 2, 0, -2, 1, 0, -1 };
                    pixelsFilteredVertical = pixels.Convolute(filter1, writeableBitmap.BackBufferStride, (int)writeableBitmap.Height, (int)writeableBitmap.Width, hBitsPerPixel, wBitsPerPixel);
                    pixelsFilteredHorizontal = pixels.Convolute(filter2, writeableBitmap.BackBufferStride, (int)writeableBitmap.Height, (int)writeableBitmap.Width, hBitsPerPixel, wBitsPerPixel);
                    pixelsFiltered = pixelsFilteredHorizontal.Select((x, index) => Math.Max(x, pixelsFilteredVertical[index])).ToArray();
                    writeableBitmap.UpdateWithPixelArray(pixelsFiltered);
                    break;
                case FiltrationMethod.Laplace:
                    filter1 = new int[] { -1, -1, -1, -1, 8, -1, -1, -1, -1 };
                    pixelsFiltered = pixels.Convolute(filter1, writeableBitmap.BackBufferStride, (int)writeableBitmap.Height, (int)writeableBitmap.Width, hBitsPerPixel, wBitsPerPixel);
                    writeableBitmap.UpdateWithPixelArray(pixelsFiltered);
                    break;
                case FiltrationMethod.Corner:
                    filter1 = new int[] { 1, 1, 1, 1, -2, -1, 1, -1, -1 };
                    pixelsFiltered = pixels.Convolute(filter1, writeableBitmap.BackBufferStride, (int)writeableBitmap.Height, (int)writeableBitmap.Width, hBitsPerPixel, wBitsPerPixel);
                    writeableBitmap.UpdateWithPixelArray(pixelsFiltered);
                    break;
                case FiltrationMethod.Kuwahara:
                    writeableBitmap.ForEachAsync((x, y, color) =>
                    {
                        var (rWindow, gWindow, bWindow) = pixels.GetWindow(5, x, y, stride, width, height);
                        if (rWindow.Length == 0) return color;

                        var filter = rWindow.Zip(gWindow, (x, y) => x + y).Zip(bWindow, (x, y) => x + y).Select(sum => sum / 3).ToArray();
                        int[] area1 = new int[] { filter[0], filter[1], filter[2], filter[5], filter[6], filter[7], filter[10], filter[11], filter[12] };
                        int[] area2 = new int[] { filter[2], filter[3], filter[4], filter[7], filter[8], filter[9], filter[12], filter[13], filter[14] };
                        int[] area3 = new int[] { filter[10], filter[11], filter[12], filter[15], filter[16], filter[17], filter[20], filter[21], filter[22] };
                        int[] area4 = new int[] { filter[12], filter[13], filter[14], filter[17], filter[18], filter[19], filter[22], filter[23], filter[24] };
                        double[] variances = new double[] { area1.Variance(), area2.Variance(), area3.Variance(), area4.Variance() };

                        double min = variances[0];
                        int minIndex = 0;
                        for (int i = 1; i < variances.Length; i++)
                            if(variances[i] < min)
                            {
                                min = variances[i];
                                minIndex = i;
                            }
                        byte[] rFilter;
                        byte[] gFilter;
                        byte[] bFilter;
                        switch (minIndex)
                        {
                            case 0:
                                rFilter = new byte[] { rWindow[0], rWindow[1], rWindow[2], rWindow[5], rWindow[6], rWindow[7], rWindow[10], rWindow[11], rWindow[12] };
                                gFilter = new byte[] { gWindow[0], gWindow[1], gWindow[2], gWindow[5], gWindow[6], gWindow[7], gWindow[10], gWindow[11], gWindow[12] };
                                bFilter = new byte[] { bWindow[0], bWindow[1], bWindow[2], bWindow[5], bWindow[6], bWindow[7], bWindow[10], bWindow[11], bWindow[12] };
                                break;
                            case 1:
                                rFilter = new byte[] { rWindow[2], rWindow[3], rWindow[4], rWindow[7], rWindow[8], rWindow[9], rWindow[12], rWindow[13], rWindow[14] };
                                gFilter = new byte[] { gWindow[2], gWindow[3], gWindow[4], gWindow[7], gWindow[8], gWindow[9], gWindow[12], gWindow[13], gWindow[14] };
                                bFilter = new byte[] { bWindow[2], bWindow[3], bWindow[4], bWindow[7], bWindow[8], bWindow[9], bWindow[12], bWindow[13], bWindow[14] };
                                break;
                            case 2:
                                rFilter = new byte[] { rWindow[10], rWindow[11], rWindow[12], rWindow[15], rWindow[16], rWindow[17], rWindow[20], rWindow[21], rWindow[22] };
                                gFilter = new byte[] { gWindow[10], gWindow[11], gWindow[12], gWindow[15], gWindow[16], gWindow[17], gWindow[20], gWindow[21], gWindow[22] };
                                bFilter = new byte[] { bWindow[10], bWindow[11], bWindow[12], bWindow[15], bWindow[16], bWindow[17], bWindow[20], bWindow[21], bWindow[22] };
                                break;
                            case 3:
                                rFilter = new byte[] { rWindow[12], rWindow[13], rWindow[14], rWindow[17], rWindow[18], rWindow[19], rWindow[22], rWindow[23], rWindow[24] };
                                gFilter = new byte[] { gWindow[12], gWindow[13], gWindow[14], gWindow[17], gWindow[18], gWindow[19], gWindow[22], gWindow[23], gWindow[24] };
                                bFilter = new byte[] { bWindow[12], bWindow[13], bWindow[14], bWindow[17], bWindow[18], bWindow[19], bWindow[22], bWindow[23], bWindow[24] };
                                break;
                            default:
                                rFilter = gFilter = bFilter = new byte[0];
                                break;
                        }

                        Array.Sort(rFilter);
                        Array.Sort(gFilter);
                        Array.Sort(bFilter);
                        return Color.FromArgb(color.A, rFilter[rFilter.Length / 2], gFilter[gFilter.Length / 2], bFilter[bFilter.Length / 2]);
                    });
                    break;
                case FiltrationMethod.Median3x3:
                    writeableBitmap.ForEachAsync((x, y, color) =>
                    {
                        var (rFilter, gFilter, bFilter) = pixels.GetWindow(3, x, y, stride, width, height);
                        if (rFilter.Length == 0) return color;
                        Array.Sort(rFilter);
                        Array.Sort(gFilter);
                        Array.Sort(bFilter);

                        return Color.FromArgb(color.A, rFilter[rFilter.Length / 2], gFilter[gFilter.Length / 2], bFilter[bFilter.Length / 2]);
                    });
                    break;
                case FiltrationMethod.Median5x5:
                    writeableBitmap.ForEachAsync((x, y, color) =>
                    {
                        var (rFilter, gFilter, bFilter) = pixels.GetWindow(5, x, y, stride, width, height);
                        if (rFilter.Length == 0) return color;
                        Array.Sort(rFilter);
                        Array.Sort(gFilter);
                        Array.Sort(bFilter);

                        return Color.FromArgb(color.A, rFilter[rFilter.Length / 2], gFilter[gFilter.Length / 2], bFilter[bFilter.Length / 2]);
                    });
                    break;
            }
            return writeableBitmap.ToBitmapSource();
        }
    }
}
