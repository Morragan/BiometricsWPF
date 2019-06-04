using Biometria.Extensions;
using Biometria.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Xml.Serialization;

namespace Biometria
{
    /// <summary>
    /// Interaction logic for KeystrokeDynamicsWindow.xaml
    /// </summary>
    public partial class KeystrokeDynamicsWindow : Window
    {
        public int? K { get; set; }
        public KDDistanceMethod DistanceMethod { get; set; } = KDDistanceMethod.Euclid;
        public ObservableCollection<KDReading> Readings { get; set; } = new ObservableCollection<KDReading>();
        public KeystrokeDynamicsWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void LoadReadings(object sender, RoutedEventArgs e)
        {
            var reader = new XmlSerializer(typeof(ObservableCollection<KDReading>));
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/KDReadings.xml";
            try
            {
                using (StreamReader file = new StreamReader(path))
                {
                    Readings = (ObservableCollection<KDReading>)reader.Deserialize(file);
                    Readings_ListView.ItemsSource = Readings;
                    var view = (CollectionView)CollectionViewSource.GetDefaultView(Readings_ListView.ItemsSource);
                    var groupDescription = new PropertyGroupDescription("Name");
                    view.GroupDescriptions.Add(groupDescription);
                }
            }
            catch (FileNotFoundException) { }
        }

        private void SaveReadings(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var writer = new XmlSerializer(typeof(ObservableCollection<KDReading>));
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/KDReadings.xml";
            using (FileStream file = File.Create(path))
            {
                writer.Serialize(file, Readings);
            }
        }

        private void OpenKDReadingWindow(object sender, RoutedEventArgs e)
        {
            var window = new KeystrokeDynamicsReadingWindow();
            window.ShowDialog();
            if (window.DialogResult != true) return;
            Readings.Add(window.Reading);
        }

        private void DeleteReading(object sender, RoutedEventArgs e)
        {
            Readings.Remove((KDReading)Readings_ListView.SelectedItem);
        }

        private void RecognizeUser(object sender, RoutedEventArgs e)
        {
            if ((K == null || K < 0) && DistanceMethod != KDDistanceMethod.Mahalanobis)
            {
                MessageBox.Show("Proszę wybrać parametr K");
                return;
            }
            var window = new KeystrokeDynamicsReadingWindow
            {
                NameTextBoxVisibility = Visibility.Collapsed
            };
            window.ShowDialog();
            if (window.DialogResult != true) return;
            var reading = window.Reading;
            string userName = reading.EvaluateUserName(Readings.ToArray(), DistanceMethod, K);
            MessageBox.Show($"Rozpoznany użytkownik to {userName}", "Uzyskano wynik!");
        }

        private void VerifyUser(object sender, RoutedEventArgs e)
        {
            if ((K == null || K < 0) && DistanceMethod != KDDistanceMethod.Mahalanobis)
            {
                MessageBox.Show("Proszę wybrać parametr K");
                return;
            }
            var window = new KeystrokeDynamicsReadingWindow();
            window.ShowDialog();
            if (window.DialogResult != true) return;
            var reading = window.Reading;
            string userName = reading.EvaluateUserName(Readings.ToArray(), DistanceMethod, K.Value);
            if (userName == window.UserName) MessageBox.Show("Pomyślnie zweryfikowano użytkownika", "Uzyskano wynik!");
            else MessageBox.Show("Wynik weryfikacji jest negatywny", "Uzyskano wynik");
        }

        private void TestMetrics(object sender, RoutedEventArgs e)
        {
            double[] euclidPerformance = new double[9],
                manhattanPerformance = new double[9],
                chebyshevPerformance = new double[9],
                mahalanobisPerformance = new double[9];

            for (int k = 1; k < 10; k++)
            {
                double euclidCorrectResultsCount = 0,
                    manhattanCorrectResultsCount = 0,
                    chebyshevCorrectResultsCount = 0,
                    mahalanobisCorrectResultsCount = 0;

                foreach (var reading in Readings)
                {
                    if (reading.EvaluateUserName(Readings.Except(reading.Yield()).ToArray(), KDDistanceMethod.Euclid, k) == reading.Name) euclidCorrectResultsCount++;
                    if (reading.EvaluateUserName(Readings.Except(reading.Yield()).ToArray(), KDDistanceMethod.Manhattan, k) == reading.Name) manhattanCorrectResultsCount++;
                    if (reading.EvaluateUserName(Readings.Except(reading.Yield()).ToArray(), KDDistanceMethod.Chebyshev, k) == reading.Name) chebyshevCorrectResultsCount++;
                    if (reading.EvaluateUserName(Readings.Except(reading.Yield()).ToArray(), KDDistanceMethod.Mahalanobis, k) == reading.Name) mahalanobisCorrectResultsCount++;
                }

                euclidPerformance[k - 1] = euclidCorrectResultsCount / Readings.Count;
                manhattanPerformance[k - 1] = manhattanCorrectResultsCount / Readings.Count;
                chebyshevPerformance[k - 1] = chebyshevCorrectResultsCount / Readings.Count;
                mahalanobisPerformance[k - 1] = mahalanobisCorrectResultsCount / Readings.Count;
            }

            var window = new KDMetricsTestResultWindow(euclidPerformance, manhattanPerformance, chebyshevPerformance, mahalanobisPerformance);
            window.Show();
        }
    }
}
