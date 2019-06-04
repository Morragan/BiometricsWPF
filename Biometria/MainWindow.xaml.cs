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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using System.ComponentModel;
using Biometria.Extensions;

namespace Biometria
{
    public partial class MainWindow : Window
    {
        byte[] pixels;
        BitmapSource savedBitmapCopy;

        public int[] brightnessHistogramValues;

        // Bindowane właściwości
        public BitmapSource BitmapSource
        {
            get => (BitmapSource)Processed_Image.Source;
            set => Processed_Image.Source = value;
        }
        public Brush RectangleFill { get; set; }
        public double Brightness { get; set; } = 1;
        public uint StretchingValA { get; set; } = 0;
        public uint StretchingValB { get; set; } = 255;

        public MainWindow()
        {
            InitializeComponent();
            RectangleFill = new SolidColorBrush(Colors.Black);
        }

        /// <summary>
        /// Pobiera obraz z wybranego pliku i zapisuje go do zmiennej typu BitmapSource. Wywoływana po kliknięciu "Otwórz"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenImage(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "JPG Files (*.jpg)|*.jpg|JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|Bitmap Files (*.bmp)|*.bmp|GIF Files (*.gif)|*.gif|TIFF Files (*.tiff)|*.tiff",
                FilterIndex = 3,
                Multiselect = false
            };

            if (ofd.ShowDialog() == false) return;
            //otwórz i odpowiednio zdekoduj obraz
            Stream imageStreamSource = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            BitmapDecoder decoder = BitmapDecoder.Create(imageStreamSource, BitmapCreateOptions.IgnoreImageCache, BitmapCacheOption.None);
            BitmapSource = decoder.Frames[0];
            if (BitmapSource.Format != PixelFormats.Bgra32)
                BitmapSource = new FormatConvertedBitmap(BitmapSource, PixelFormats.Bgra32, null, 0);
            savedBitmapCopy = BitmapSource.Clone();
            Main_Canvas.DataContext = BitmapSource;
            // zresetuj skalę
            Zoom_Slider.Value = 1;
            // wyłącz blurring
            Processed_Image.UseLayoutRounding = true;
            RenderOptions.SetBitmapScalingMode(Processed_Image, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(Processed_Image, EdgeMode.Aliased);
            Processed_Image.Stretch = Stretch.None;
            // ustaw focus na canvas
            Main_Canvas.Focus();

            // włącz kontrolki
            SaveAs_MenuOption.IsEnabled = true;
            Histogram_MenuOption.IsEnabled = true;
            Zoom_Slider.IsEnabled = true;
            Parameters_GroupBox.IsEnabled = true;
            Binarization_MenuOption.IsEnabled = true;
            Filtration_MenuOption.IsEnabled = true;

            // przeładowanie bitmapy
            ChangePixel(sender, new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left) { RoutedEvent = Button.ClickEvent });
            ChangePixel(sender, new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left) { RoutedEvent = Button.ClickEvent });
        }

        /// <summary>
        /// Jeżeli wciśnięty jest klawisz ctrl, zmienia kolor piksela pod kursorem na wybrany. 
        /// Jeżeli nie, ustawia wybrany kolor na kolor piksela pod kursorem. Wywoływana po kliknięciu canvasu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangePixel(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(Processed_Image);

            var writeableBitmap = new WriteableBitmap(BitmapSource);
            int stride = writeableBitmap.BackBufferStride;
            int arraySize = stride * BitmapSource.PixelHeight;
            pixels = new byte[arraySize];
            // TODO: Zamienić wszystkie obliczenia indexów na poprawne, zgodne z poniższym
            int index = writeableBitmap.GetIndex((int)p.X, (int)p.Y);

            writeableBitmap.CopyPixels(pixels, stride, 0);

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                pixels.SetRGB((int)p.X, (int)p.Y,
                    ((SolidColorBrush)RectangleFill).Color.R, ((SolidColorBrush)RectangleFill).Color.G, ((SolidColorBrush)RectangleFill).Color.B, writeableBitmap.BackBufferStride);
            else
            {
                // ustaw kolor pędzla na kolor wybranego piksela
                try
                {
                    R_Slider.Value = pixels[index + 2];
                    G_Slider.Value = pixels[index + 1];
                    B_Slider.Value = pixels[index];
                }
                catch (IndexOutOfRangeException)
                {
                    R_Slider.Value = 255;
                    G_Slider.Value = 255;
                    B_Slider.Value = 255;
                }
            }
            // zapisz do bitmapy
            Int32Rect rect = new Int32Rect(0, 0, writeableBitmap.PixelWidth, writeableBitmap.PixelHeight);
            writeableBitmap.WritePixels(rect, pixels, stride, 0);
            BitmapSource = writeableBitmap.ToBitmapSource();
        }

        /// <summary>
        /// Zapisuje bitmapę do pliku z wybranym formatowaniem. Wywoływana po kliknięciu "Zapisz jako"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveToFile(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "JPEG Files (*.jpg)|*.jpg|PNG Files (*.png)|*.png|Bitmap Files (*.bmp)|*.bmp|GIF Files (*.gif)|*.gif|TIFF Files (*.tiff)|*.tiff";

            if (sfd.ShowDialog() == true)
            {
                //zapis do pliku sfd.FileName
                BitmapEncoder encoder;
                switch (sfd.FileName.Split('.').Last())
                {
                    case "jpg":
                        encoder = new JpegBitmapEncoder();
                        break;
                    case "png":
                        encoder = new PngBitmapEncoder();
                        break;
                    case "bmp":
                        encoder = new BmpBitmapEncoder();
                        break;
                    case "gif":
                        encoder = new GifBitmapEncoder();
                        break;
                    case "tiff":
                        encoder = new TiffBitmapEncoder();
                        break;
                    default:
                        encoder = new TiffBitmapEncoder();
                        break;
                }

                SaveUsingEncoder(Processed_Image, sfd.FileName, encoder);
            }
        }

        /// <summary>
        /// Zapisuje obraz do pliku przy użyciu wybranego kodera
        /// </summary>
        /// <param name="visual"></param>
        /// <param name="fileName"></param>
        /// <param name="encoder"></param>
        private void SaveUsingEncoder(FrameworkElement visual, string fileName, BitmapEncoder encoder)
        {
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)visual.ActualWidth, (int)visual.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            BitmapFrame frame = BitmapFrame.Create(bitmap);
            encoder.Frames.Add(frame);

            using (var stream = File.Create(fileName))
            {
                encoder.Save(stream);
            }
        }

        /// <summary>
        /// Zmienia wybrany kolor. Wywoływana po kliknięciu koloru w kontrolce color picker
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClrPcker_Background_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            // jeżeli jeszcze nie zainicjalizowany, nic nie rób
            if (Brush_Rectangle == null) return;
            ((SolidColorBrush)RectangleFill).Color = ClrPcker_Background.SelectedColor.Value;
            R_Slider.Value = ((SolidColorBrush)RectangleFill).Color.R;
            G_Slider.Value = ((SolidColorBrush)RectangleFill).Color.G;
            B_Slider.Value = ((SolidColorBrush)RectangleFill).Color.B;
        }

        /// <summary>
        /// Rozwija kontrolkę color picker. Wywoływana po kliknięciu pola z wybranym pikselem
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenColorPicker(object sender, MouseButtonEventArgs e)
        {
            ClrPcker_Background.IsOpen = true;
            e.Handled = true;
        }

        /// <summary>
        /// Modyfikuje wartość barwy składowej R wybranego koloru na podstawie wartości suwaka. Wywoływana po zmianie wartości suwaka R
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetColorFromRSlider(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ((SolidColorBrush)RectangleFill).Color = Color.FromRgb((byte)R_Slider.Value, ((SolidColorBrush)RectangleFill).Color.G, ((SolidColorBrush)RectangleFill).Color.B);
        }

        /// <summary>
        /// Modyfikuje wartość barwy składowej G wybranego koloru na podstawie wartości suwaka. Wywoływana po zmianie wartości suwaka G
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetColorFromGSlider(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ((SolidColorBrush)RectangleFill).Color = Color.FromRgb(((SolidColorBrush)RectangleFill).Color.R, (byte)G_Slider.Value, ((SolidColorBrush)RectangleFill).Color.B);
        }

        /// <summary>
        /// Modyfikuje wartość barwy składowej B wybranego koloru na podstawie wartości suwaka. Wywoływana po zmianie wartości suwaka B
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetColorFromBSlider(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ((SolidColorBrush)RectangleFill).Color = Color.FromRgb(((SolidColorBrush)RectangleFill).Color.R, ((SolidColorBrush)RectangleFill).Color.G, (byte)B_Slider.Value);
        }

        /// <summary>
        /// Zamyka program
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Otwiera okno z histogramami i wypełnia zmienne przechowujące tablice z wartościami histogramów. Wywoływana po kliknięciu "Histogramy"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowHistograms(object sender, RoutedEventArgs e)
        {
            Stretching_GroupBox.IsEnabled = true;
            EqualizeHistogram_MenuOption.IsEnabled = true;

            HistogramsWindow window = new HistogramsWindow(BitmapSource);
            window.ShowDialog();

            brightnessHistogramValues = BitmapSource.BrightnessHistogram();
            HStretch_a_TextBox.Text = Array.FindIndex(brightnessHistogramValues, val => val == brightnessHistogramValues.First(i => i != 0)).ToString();
            HStretch_b_TextBox.Text = Array.LastIndexOf(brightnessHistogramValues, brightnessHistogramValues.Last(i => i != 0)).ToString();
        }

        /// <summary>
        /// Resetuje obraz do stanu, w jakim został wczytany z pliku. Wywoływana po kliknięciu guzika "Reset"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetImage(object sender, RoutedEventArgs e)
        {
            BitmapSource = savedBitmapCopy.Clone();
        }

        /// <summary>
        /// Zmienia jasność obrazu używając funkcji potęgowej z podanym parametrem. Wywoływana po kliknięciu guzika "Zmień"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeBrightness(object sender, RoutedEventArgs e)
        {
            var writeableBitmap = new WriteableBitmap(BitmapSource);
            var LUT = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                uint val = (uint)Math.Pow(i, Brightness);
                LUT[i] = (byte)(val > 255 ? 255 : val);
            }

            writeableBitmap.ForEach((x, y, color) => Color.FromArgb(color.A, LUT[color.R], LUT[color.G], LUT[color.B]));

            // zapisz do bitmapy
            BitmapSource = writeableBitmap.ToBitmapSource();
        }

        /// <summary>
        /// Wykonuje operację rozciągania histogramu na wybranym obrazie, używając parametrów a i b. Wywoływana po kliknięciu guzika "Rozciągnij"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StretchHistogram(object sender, RoutedEventArgs e)
        {
            var LUT = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                int val = (int)(255 * (i - StretchingValA) / (StretchingValB - StretchingValA));
                if (val < 0) val = (int)StretchingValA;
                if (val > 255) val = (int)StretchingValB;
                LUT[i] = (byte)val;
            }
            var writeableBitmap = new WriteableBitmap(BitmapSource);
            writeableBitmap.ForEach((x, y, color) => Color.FromArgb(color.A, LUT[color.R], LUT[color.G], LUT[color.B]));

            // zapisz do bitmapy
            BitmapSource = writeableBitmap.ToBitmapSource();
        }

        private void OpenBinarizationWindow(object sender, RoutedEventArgs e)
        {
            BinarizationWindow window = new BinarizationWindow(BitmapSource.Grayscale());
            window.ShowDialog();
        }

        /// <summary>
        /// Wykonuje operację wyrównania histogramu. Wywoływana po kliknięciu guzika "Wyrównaj"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EqualizeHistogram(object sender, RoutedEventArgs e)
        {
            var distribution = new double[256];
            var LUT = new byte[256];
            int sum = brightnessHistogramValues.Sum();
            for (int i = 0; i < 256; i++)
            {
                distribution[i] = (double)brightnessHistogramValues.Take(i + 1).Sum() / sum;

            }
            double d0 = distribution.First(val => val != 0);
            for (int i = 0; i < 256; i++)
            {
                var val = (distribution[i] - d0) / (1 - d0) * 255;
                if (val < 0) val = 0;
                if (val > 255) val = 255;
                LUT[i] = (byte)val;
            }

            var writeableBitmap = new WriteableBitmap(BitmapSource);
            writeableBitmap.ForEach((x, y, color) => Color.FromArgb(color.A, LUT[color.R], LUT[color.G], LUT[color.B]));

            // zapisz do bitmapy
            BitmapSource = writeableBitmap.ToBitmapSource();
        }

        private void OpenFiltrationWindow(object sender, RoutedEventArgs e)
        {
            FiltrationWindow window = new FiltrationWindow(BitmapSource);
            window.ShowDialog();
        }

        private void ThinImageKMM(object sender, RoutedEventArgs e)
        {
            BitmapSource = BitmapSource.Binarize(BitmapSource.OtsuThreshold()).KMMThinning();
            var minutiae = BitmapSource.GetMinutiae();
            var writeableBitmap = new WriteableBitmap(BitmapSource);
            writeableBitmap.ForEach((x, y, color) =>
            {
                if (minutiae.Contains((x, y)))
                    return Colors.Red;
                else return color;
            });
            BitmapSource = writeableBitmap.ToBitmapSource();
        }

        private void ThinImageK3M(object sender, RoutedEventArgs e)
        {
            BitmapSource = BitmapSource.Binarize(BitmapSource.OtsuThreshold()).K3MThinning();
            var minutiae = BitmapSource.GetMinutiae();
            var writeableBitmap = new WriteableBitmap(BitmapSource);
            writeableBitmap.ForEach((x, y, color) =>
            {
                if (minutiae.Contains((x, y)))
                    return Colors.Red;
                else return color;
            });
            BitmapSource = writeableBitmap.ToBitmapSource();
        }

        private void OpenKeystrokeDynamicsWindow(object sender, RoutedEventArgs e)
        {
            var window = new KeystrokeDynamicsWindow();
            window.Show();
        }
    }
}