using System.IO;
using System.Windows.Media.Imaging;

namespace Biometria.Extensions
{
    public static class ConversionExtensions
    {
        /// <summary>
        /// Zamienia edytowalną bitmapę na taką, której można użyć jako zdjęcia
        /// </summary>
        /// <param name="wbm"></param>
        /// <returns></returns>
        public static BitmapSource ToBitmapSource(this WriteableBitmap wbm)
        {
            BitmapImage bmImage = new BitmapImage();
            using (MemoryStream stream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(wbm));
                encoder.Save(stream);
                bmImage.BeginInit();
                bmImage.CacheOption = BitmapCacheOption.OnLoad;
                bmImage.StreamSource = stream;
                bmImage.EndInit();
                bmImage.Freeze();
            }
            return bmImage;
        }
    }
}
