using System;
using System.Globalization;
using System.Windows.Data;

namespace Biometria.Converters
{
    class CanvasSizeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is double) return (double)values[0] * (double)values[1];
            else return 100;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
