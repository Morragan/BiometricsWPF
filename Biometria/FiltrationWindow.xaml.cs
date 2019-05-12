using Biometria.Extensions;
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
    /// Interaction logic for FiltrationWindow.xaml
    /// </summary>
    /// 
    public partial class FiltrationWindow : Window
    {
        readonly BitmapSource bitmapSourceCopy;

        public BitmapSource BitmapSource { get; set; }
        #region 3x3 Mask
        public int Mask3x30 { get; set; }
        public int Mask3x31 { get; set; }
        public int Mask3x32 { get; set; }
        public int Mask3x33 { get; set; }
        public int Mask3x34 { get; set; }
        public int Mask3x35 { get; set; }
        public int Mask3x36 { get; set; }
        public int Mask3x37 { get; set; }
        public int Mask3x38 { get; set; }
        #endregion
        public int[] Mask3x3 => new int[9] { Mask3x30, Mask3x31, Mask3x32, Mask3x33, Mask3x34, Mask3x35, Mask3x36, Mask3x37, Mask3x38 };
        #region 5x5 Mask
        public int Mask5x50 { get; set; }
        public int Mask5x51 { get; set; }
        public int Mask5x52 { get; set; }
        public int Mask5x53 { get; set; }
        public int Mask5x54 { get; set; }
        public int Mask5x55 { get; set; }
        public int Mask5x56 { get; set; }
        public int Mask5x57 { get; set; }
        public int Mask5x58 { get; set; }
        public int Mask5x59 { get; set; }
        public int Mask5x510 { get; set; }
        public int Mask5x511 { get; set; }
        public int Mask5x512 { get; set; }
        public int Mask5x513 { get; set; }
        public int Mask5x514 { get; set; }
        public int Mask5x515 { get; set; }
        public int Mask5x516 { get; set; }
        public int Mask5x517 { get; set; }
        public int Mask5x518 { get; set; }
        public int Mask5x519 { get; set; }
        public int Mask5x520 { get; set; }
        public int Mask5x521 { get; set; }
        public int Mask5x522 { get; set; }
        public int Mask5x523 { get; set; }
        public int Mask5x524 { get; set; }
        #endregion
        public int[] Mask5x5 => new int[25] { Mask5x50, Mask5x51, Mask5x52, Mask5x53, Mask5x54, Mask5x55, Mask5x56, Mask5x57, Mask5x58, Mask5x59, Mask5x510, Mask5x511, Mask5x512, Mask5x513, Mask5x514, Mask5x515, Mask5x516, Mask5x517, Mask5x518, Mask5x519, Mask5x520, Mask5x521, Mask5x522, Mask5x523, Mask5x524 };

        public FiltrationWindow(BitmapSource bitmapSource)
        {
            BitmapSource = bitmapSource;
            bitmapSourceCopy = bitmapSource.Clone();
            InitializeComponent();
            DataContext = this;

            // wyłącz rozmycie
            Processed_Image.UseLayoutRounding = true;
            RenderOptions.SetBitmapScalingMode(Processed_Image, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(Processed_Image, EdgeMode.Aliased);
            Processed_Image.Stretch = Stretch.None;
        }

        private void ResetImage(object sender, RoutedEventArgs e)
        {
            BitmapSource = bitmapSourceCopy.Clone();
            Processed_Image.Source = BitmapSource;
        }

        private void FilterImage(object sender, RoutedEventArgs e)
        {
            FiltrationMethod method;
            #region Set method
            if (LowPass_RB.IsChecked == true) method = FiltrationMethod.LowPass;
            else if (Prewitt_RB.IsChecked == true) method = FiltrationMethod.Prewitt;
            else if (Sobel_RB.IsChecked == true) method = FiltrationMethod.Sobel;
            else if (Laplace_RB.IsChecked == true) method = FiltrationMethod.Laplace;
            else if (Corner_RB.IsChecked == true) method = FiltrationMethod.Corner;
            else if (Kuwahara_RB.IsChecked == true) method = FiltrationMethod.Kuwahara;
            else if (Median3x3_RB.IsChecked == true) method = FiltrationMethod.Median3x3;
            else method = FiltrationMethod.Median5x5;
            #endregion
            //TODO: Sprawdzić czy zadziała bez przypisania (samo BitmapSource.Filter())
            if (Custom3x3_RB.IsChecked == true) BitmapSource = BitmapSource.FilterCustom(Mask3x3);
            else if (Custom5x5_RB.IsChecked == true) BitmapSource = BitmapSource.FilterCustom(Mask5x5);
            else BitmapSource = BitmapSource.Filter(method);
            Processed_Image.Source = BitmapSource;
        }
    }
}
