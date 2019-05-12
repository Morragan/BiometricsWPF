using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Biometria.Extensions;
using LiveCharts;
using LiveCharts.Wpf;

namespace Biometria
{
    /// <summary>
    /// Logika interakcji dla klasy HistogramsWindow.xaml
    /// </summary>
    public partial class HistogramsWindow : Window
    {
        BitmapSource BitmapSource;
        
        public SeriesCollection CollectionR { get; set; } = new SeriesCollection();
        public SeriesCollection CollectionG { get; set; } = new SeriesCollection();
        public SeriesCollection CollectionB { get; set; } = new SeriesCollection();
        public SeriesCollection CollectionBrightness { get; set; } = new SeriesCollection();

        public HistogramsWindow(BitmapSource bitmapSource)
        {
            InitializeComponent();
            BitmapSource = bitmapSource;
            DataContext = this;
        }
        /// <summary>
        /// Oblicza i zapisuje histogramy do zmiennych typu SeriesCollection, wywoływana przy załadowaniu okna
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadHistograms(object sender, RoutedEventArgs e)
        {
            int[] rHistogramValues, gHistogramValues, bHistogramValues, brightnessHistogramValues;
            (rHistogramValues, gHistogramValues, bHistogramValues, brightnessHistogramValues) = BitmapSource.Histogram();
            
            CollectionR.Add(new LineSeries
            {
                Values = new ChartValues<int>(rHistogramValues),
                LineSmoothness = 0
            });
            CollectionG.Add(new LineSeries
            {
                Values = new ChartValues<int>(gHistogramValues),
                LineSmoothness = 0
            });
            CollectionB.Add(new LineSeries
            {
                Values = new ChartValues<int>(bHistogramValues),
                LineSmoothness = 0
            });
            CollectionBrightness.Add(new LineSeries
            {
                Values = new ChartValues<int>(brightnessHistogramValues),
                LineSmoothness = 0
            });
        }
    }
}
