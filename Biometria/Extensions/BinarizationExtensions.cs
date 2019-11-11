using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Biometria.Extensions
{
    public static class BinarizationExtensions
    {
        public static int OtsuThreshold(this BitmapSource bitmapSource)
        {
            int[] histogram = bitmapSource.BrightnessHistogram();
            int histogramSum = histogram.Sum();
            double preThresholdProbability = 0;
            double postThresholdProbability = 1;
            Dictionary<int, double> withinClassVariances = new Dictionary<int, double>();
            for (int potentialThreshold = 0; potentialThreshold < histogram.Length; potentialThreshold++)
            {
                preThresholdProbability += (double)histogram[potentialThreshold] / histogramSum;
                postThresholdProbability -= (double)histogram[potentialThreshold] / histogramSum;
                // TODO: Zamienić wszystko na prawdopodobieństwa
                if (preThresholdProbability == 0) continue;
                if (postThresholdProbability == 0) break;

                double preThresholdAvg = 0;
                double postThresholdAvg = 0;
                for (int i = 0; i <= potentialThreshold; i++) preThresholdAvg += (double)histogram[i] / histogramSum * i;
                for (int i = potentialThreshold + 1; i < histogram.Length; i++) postThresholdAvg += (double)histogram[i] / histogramSum * i;
                preThresholdAvg /= preThresholdProbability;
                postThresholdAvg /= postThresholdProbability;

                double preThresholdVariance = 0;
                double postThresholdVariance = 0;
                for (int i = 0; i <= potentialThreshold; i++) preThresholdVariance += (i - preThresholdAvg) * (i - preThresholdAvg) * ((double)histogram[i] / histogramSum);
                for (int i = potentialThreshold + 1; i < histogram.Length; i++) postThresholdVariance += (i - postThresholdAvg) * (i - postThresholdAvg) * ((double)histogram[i] / histogramSum);
                preThresholdVariance /= preThresholdProbability;
                postThresholdVariance /= postThresholdProbability;

                double withinClassVariance = preThresholdProbability * preThresholdVariance + postThresholdProbability * postThresholdVariance;
                withinClassVariances.Add(potentialThreshold, withinClassVariance);
            }
            return withinClassVariances.Aggregate((l, r) => l.Value < r.Value ? l : r).Key;
        }

        public static byte BlackPercentageThreshold(this BitmapSource bitmapSource, int percentage)
        {
            int[] histogram = bitmapSource.BrightnessHistogram();
            int allPixelsCount = histogram.Sum(), pixelsCount = 0;
            double nPercentage = Convert.ToDouble(percentage) / 100.0;
            var blackPixelsCount = allPixelsCount * nPercentage;
            for (byte threshold = 0; threshold <= 255; threshold++)
            {
                pixelsCount += histogram[threshold];
                if (pixelsCount >= blackPixelsCount) return threshold;
            }
            return 255;
        }

        public static byte EntropyThreshold(this BitmapSource bitmapSource)
        {
            int[] Histogram = bitmapSource.BrightnessHistogram();
            int X, Y, Amount = 0;
            double[] HistGramD = new double[256];
            double SumIntegral, EntropyBack, EntropyFore, MaxEntropy;
            int MinValue, MaxValue;
            int Threshold = 0;

            for (MinValue = 0; MinValue < 256 && Histogram[MinValue] == 0; MinValue++) ;
            for (MaxValue = 255; MaxValue > MinValue && Histogram[MinValue] == 0; MaxValue--) ;
            if (MaxValue == MinValue) return Convert.ToByte(MaxValue);
            if (MinValue + 1 == MaxValue) return Convert.ToByte(MinValue);

            for (Y = MinValue; Y <= MaxValue; Y++) Amount += Histogram[Y];

            for (Y = MinValue; Y <= MaxValue; Y++) HistGramD[Y] = (double)Histogram[Y] / Amount + 1e-17;

            MaxEntropy = double.MinValue;
            for (Y = MinValue + 1; Y < MaxValue; Y++)
            {
                SumIntegral = 0;
                for (X = MinValue; X <= Y; X++) SumIntegral += HistGramD[X];
                EntropyBack = 0;
                for (X = MinValue; X <= Y; X++) EntropyBack += (-HistGramD[X] / SumIntegral * Math.Log(HistGramD[X] / SumIntegral));
                EntropyFore = 0;
                for (X = Y + 1; X <= MaxValue; X++) EntropyFore += (-HistGramD[X] / (1 - SumIntegral) * Math.Log(HistGramD[X] / (1 - SumIntegral)));
                if (MaxEntropy < EntropyBack + EntropyFore)
                {
                    Threshold = Y;
                    MaxEntropy = EntropyBack + EntropyFore;
                }
            }
            return Convert.ToByte(Threshold);
        }

        public static byte MeanIterativeThreshold(this BitmapSource bitmapSource)
        {
            int[] Histogram = bitmapSource.BrightnessHistogram();
            int X;
            int MeanValueOne, MeanValueTwo, SumOne, SumTwo, SumIntegralOne, SumIntegralTwo;
            int MinValue, MaxValue;
            int Threshold, NewThreshold;

            for (MinValue = 0; MinValue < 256 && Histogram[MinValue] == 0; MinValue++) ;
            for (MaxValue = 255; MaxValue > MinValue && Histogram[MinValue] == 0; MaxValue--) ;

            if (MaxValue == MinValue) return Convert.ToByte(MaxValue);
            if (MinValue + 1 == MaxValue) return Convert.ToByte(MinValue);

            Threshold = MinValue;
            NewThreshold = (MaxValue + MinValue) >> 1;
            for (int i = 0; Threshold != NewThreshold; i++)
            {
                SumOne = 0; SumIntegralOne = 0;
                SumTwo = 0; SumIntegralTwo = 0;
                Threshold = NewThreshold;
                for (X = MinValue; X <= Threshold; X++)
                {
                    SumIntegralOne += Histogram[X] * X;
                    SumOne += Histogram[X];
                }
                MeanValueOne = SumIntegralOne / SumOne;
                for (X = Threshold + 1; X <= MaxValue; X++)
                {
                    SumIntegralTwo += Histogram[X] * X;
                    SumTwo += Histogram[X];
                }
                MeanValueTwo = SumIntegralTwo / SumTwo;
                NewThreshold = (MeanValueOne + MeanValueTwo) >> 1;
            }
            return Convert.ToByte(Threshold);
        }

        public static BitmapSource Binarize(this BitmapSource bitmapSource, int threshold)
        {
            var writeableBitmap = new WriteableBitmap(bitmapSource);
            writeableBitmap.ForEachAsync((x, y, color) =>
            {
                if ((color.R + color.G + color.B) / 3 > threshold)
                    return Color.FromArgb(color.A, 255, 255, 255);
                else
                    return Color.FromArgb(color.A, 0, 0, 0);
            });
            bitmapSource = writeableBitmap.ToBitmapSource();
            return bitmapSource;
        }

        public static BitmapSource BinarizeNiblack(this BitmapSource bitmapSource, double k, int windowWidth, int windowHeight)
        {
            var writeableBitmap = new WriteableBitmap(bitmapSource);
            int stride = writeableBitmap.BackBufferStride;
            int arraySize = stride * bitmapSource.PixelHeight;
            var pixels = new byte[arraySize];
            double xBitRate = writeableBitmap.PixelHeight / writeableBitmap.Height,
                yBitRate = writeableBitmap.PixelWidth / writeableBitmap.Width;
            writeableBitmap.CopyPixels(pixels, stride, 0);
            writeableBitmap.ForEachAsync((x, y, color) =>
            {
                byte niblackThreshold = NiblackThreshold(x, y, k, windowWidth, windowHeight, pixels, stride, xBitRate, yBitRate);
                if ((color.R + color.G + color.B) / 3 > niblackThreshold)
                    return Color.FromArgb(color.A, 255, 255, 255);
                else
                    return Color.FromArgb(color.A, 0, 0, 0);
            });
            bitmapSource = writeableBitmap.ToBitmapSource();
            return bitmapSource;
        }
        /// <summary>
        /// Zwraca próg binaryzacji wyznaczony metodą Niblacka
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="k"></param>
        /// <param name="windowWidth"></param>
        /// <param name="windowHeight"></param>
        /// <param name="pixels"></param>
        /// <param name="stride"></param>
        /// <param name="xBitRate"></param>
        /// <param name="yBitRate"></param>
        /// <returns></returns>
        private static byte NiblackThreshold(int x, int y, double k, int windowWidth, int windowHeight, byte[] pixels, int stride, double xBitRate, double yBitRate)
        {
            List<double> blockValues = new List<double>();
            int arraySize = pixels.Count();
            for (int i = x - windowWidth / 2; i <= x + windowWidth / 2; i++)
                for (int j = y - windowHeight / 2; j <= y + windowHeight / 2; j++)
                {
                    int index = (int)(j * yBitRate * stride + 4 * i * xBitRate);
                    if (index >= arraySize || index < 0) continue;
                    blockValues.Add(pixels[index]);
                }
            double average = blockValues.Average();
            double sumOfSquaresOfDifferences = blockValues.Select(val => (val - average) * (val - average)).Sum();
            double standardDeviation = Math.Sqrt(sumOfSquaresOfDifferences / blockValues.Count);

            return (byte)(average + k * standardDeviation);
        }
    }
}
