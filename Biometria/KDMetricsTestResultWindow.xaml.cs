using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
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

namespace Biometria
{
    /// <summary>
    /// Interaction logic for KDMetricsTestResultWindow.xaml
    /// </summary>
    public partial class KDMetricsTestResultWindow : Window
    {
        public SeriesCollection Collection { get; set; } = new SeriesCollection();
        public KDMetricsTestResultWindow(double[] euclidPerformance, double[] manhattanPerformance, double[] chebyshevPerformance, double[] mahalanobisPerformance)
        {
            InitializeComponent();
            DataContext = this;
            var mapper = new CartesianMapper<double>()
                .X((value, index) => index + 1)
                .Y((value, index) => value);
            Charting.For<double>(mapper);
            Collection.Add(new LineSeries
            {
                Title = "Euklides",
                Values = new ChartValues<double>(euclidPerformance),
                LineSmoothness = 0.01, //0: straight lines, 1: really smooth lines
            });
            Collection.Add(new LineSeries
            {
                Title = "Manhattan",
                Values = new ChartValues<double>(manhattanPerformance),
                LineSmoothness = 0.01, //0: straight lines, 1: really smooth lines
            });
            Collection.Add(new LineSeries
            {
                Title = "Czebyszew",
                Values = new ChartValues<double>(chebyshevPerformance),
                LineSmoothness = 0.01, //0: straight lines, 1: really smooth lines
            });
            Collection.Add(new LineSeries
            {
                Title = "Mahalanobis",
                Values = new ChartValues<double>(mahalanobisPerformance),
                LineSmoothness = 0.01, //0: straight lines, 1: really smooth lines
            });
        }
    }
}
