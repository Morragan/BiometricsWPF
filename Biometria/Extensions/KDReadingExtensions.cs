using Accord.Math;
using Biometria.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Biometria.Extensions
{
    public static class KDReadingExtensions
    {
        public static double GetEuclideanDistance(this KDReading _reading, KDReading reading)
        {
            double sum = 0;
            for (int i = 0; i < 26; i++)
                sum += (_reading.LetterMeasurements[i] - reading.LetterMeasurements[i]) * (_reading.LetterMeasurements[i] - reading.LetterMeasurements[i]);
            return Math.Sqrt(sum);
        }
        public static double GetManhattanDistance(this KDReading _reading, KDReading reading)
        {
            double sum = 0;
            for (int i = 0; i < 26; i++)
                sum += Math.Abs(_reading.LetterMeasurements[i] - reading.LetterMeasurements[i]);
            return sum;
        }
        public static double GetChebyshevDistance(this KDReading _reading, KDReading reading)
        {
            double max = 0;
            for (int i = 0; i < 26; i++)
            {
                int difference = Math.Abs(_reading.LetterMeasurements[i] - reading.LetterMeasurements[i]);
                if (difference > max) max = difference;
            }
            return max;
        }
        public static double GetMahalanobisDistance(this KDReading _reading, KDReading[] userReadings)
        {
            int vectorSize = _reading.LetterMeasurements.Length,
                setSize = userReadings.Length;
            double[] v1 = Array.ConvertAll(_reading.LetterMeasurements, item => (double)item);
            var vectors = userReadings.Select(reading => reading.LetterMeasurements);
            var meanVector = vectors.Select(vector => Array.ConvertAll(vector, val => (double)val)).Mean();
            var vectorOfDistancesFromMean = v1.Zip(meanVector, (a, b) => a - b);
            double[,] matrixOfDistancesFromMean = new double[1, vectorSize],
                matrixOfDistancesFromMeanTransposed = new double[vectorSize, 1];

            for (int i = 0; i < vectorSize; i++)
                matrixOfDistancesFromMean[0, i] = matrixOfDistancesFromMeanTransposed[i, 0] = vectorOfDistancesFromMean.ElementAt(i);

            var covarianceMatrix = new double[vectorSize, vectorSize];
            covarianceMatrix.Initialize();

            for (int i = 0; i < setSize; i++)
            {
                var sample = Array.ConvertAll(userReadings[i].LetterMeasurements, val => (double)val);
                var sampleDistanceFromMean = sample.Zip(meanVector, (a, b) => a - b);
                double[,] matrixSampleDistanceFromMean = new double[1, vectorSize],
                matrixSampleDistanceFromMeanTransposed = new double[vectorSize, 1];

                for (int j = 0; j < vectorSize; j++)
                    matrixSampleDistanceFromMean[0, j] = matrixSampleDistanceFromMeanTransposed[j, 0] = sampleDistanceFromMean.ElementAt(j);

                var product = matrixSampleDistanceFromMeanTransposed.Dot(matrixSampleDistanceFromMean);
                covarianceMatrix = covarianceMatrix.Add(product);
            }
            covarianceMatrix = covarianceMatrix.Divide(setSize);
            try
            {
                covarianceMatrix = covarianceMatrix.Inverse();
            }
            catch (Exception)
            {
                covarianceMatrix = covarianceMatrix.PseudoInverse();
            }

            var outputMatrix = matrixOfDistancesFromMean.Dot(covarianceMatrix).Dot(matrixOfDistancesFromMeanTransposed);
            return Math.Sqrt(outputMatrix[0, 0]);
        }
        public static string EvaluateUserName(this KDReading _reading, KDReading[] userReadings, KDDistanceMethod distanceMethod, int? k)
        {
            IEnumerable<KDReading> orderedReadings;
            switch (distanceMethod)
            {
                case KDDistanceMethod.Euclid:
                    orderedReadings = userReadings.OrderBy(reading => reading.GetEuclideanDistance(_reading));
                    break;
                case KDDistanceMethod.Manhattan:
                    orderedReadings = userReadings.OrderBy(reading => reading.GetManhattanDistance(_reading));
                    break;
                case KDDistanceMethod.Chebyshev:
                    orderedReadings = userReadings.OrderBy(reading => reading.GetChebyshevDistance(_reading));
                    break;
                case KDDistanceMethod.Mahalanobis:
                    var groupedReadings = userReadings.GroupBy(reading => reading.Name);
                    var orderedReadingsGroupes = groupedReadings.OrderBy(group => _reading.GetMahalanobisDistance(group.ToArray()));
                    var user = orderedReadingsGroupes.First().First().Name;
                    //var output = new System.Text.StringBuilder();
                    //foreach (var group in orderedReadingsGroupes)
                    //    output.Append(" " + group.First().Name + " " + _reading.GetMahalanobisDistance(group.ToArray()).ToString() + "\n");
                    //System.Windows.MessageBox.Show(output.ToString());
                    return user;
                default:
                    throw new ArgumentNullException("No distance calculation method selected");
            }
            var selectedReadings = orderedReadings.Take(k.Value).GroupBy(reading => reading.Name);
            var maxCount = selectedReadings.Max(readings => readings.Count());
            var recognizedUser = selectedReadings.Where(readings => readings.Count() == maxCount).First().First().Name;
            return recognizedUser;
        }
    }
}
