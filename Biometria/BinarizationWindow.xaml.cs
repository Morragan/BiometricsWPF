using Biometria.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Logika interakcji dla klasy BinarizationWindow.xaml
    /// </summary>
    public partial class BinarizationWindow : Window, INotifyPropertyChanged
    {
        readonly BitmapSource bitmapSourceCopy;

        public BitmapSource BitmapSource
        {
            get => (BitmapSource)Processed_Image.Source;
            set => Processed_Image.Source = value;
        }
        private byte otsuThreshold = 0;
        public byte OtsuThreshold
        {
            get { return otsuThreshold; }
            set
            {
                otsuThreshold = value;
                NotifyPropertyChanged("OtsuThreshold");
            }
        }
        public double NiblackK { get; set; }
        public uint NiblackHeight { get; set; }
        public uint NiblackWidth { get; set; }

        public BinarizationWindow(BitmapSource bitmapSource)
        {
            InitializeComponent();
            DataContext = this;
            BitmapSource = bitmapSource;
            bitmapSourceCopy = bitmapSource.Clone();

            // wyłącz rozmycie
            Processed_Image.UseLayoutRounding = true;
            RenderOptions.SetBitmapScalingMode(Processed_Image, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(Processed_Image, EdgeMode.Aliased);
            Processed_Image.Stretch = Stretch.None;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Wyznacza metodą Otsu próg binaryzacji dla załadowanej bitmapy i wpisuje wynik do Slidera OtsuThreshold_Slider. Wywoływana po kliknięciu przycisku "Wyznacz"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EstimateOtsuThreshold(object sender, RoutedEventArgs e)
        {
            OtsuThreshold_Slider.Value = BitmapSource.OtsuThreshold();
        }

        /// <summary>
        /// Binaryzuje załadowaną bitmapę metodą Otsu lub Niblacka, w zależności od wybranego RadioButtona. Wywoływana po kliknięciu przycisku "Wykonaj"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BinarizeImage(object sender, RoutedEventArgs e)
        {
            if (Otsu_RadioButton.IsChecked == true)
                BitmapSource = BitmapSource.Binarize(OtsuThreshold);
            else
                BitmapSource = BitmapSource.BinarizeNiblack(NiblackK, (int)NiblackWidth, (int)NiblackHeight);
        }

        /// <summary>
        /// Resetuje bitmapę do stanu po wczytaniu. Wywoływana po kliknięciu przycisku "Reset"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetImage(object sender, RoutedEventArgs e)
        {
            BitmapSource = bitmapSourceCopy.Clone();
        }
    }
}
