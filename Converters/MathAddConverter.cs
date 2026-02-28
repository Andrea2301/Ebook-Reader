using System;
using System.Globalization;
using System.Windows.Data;

namespace Ebook_Reader.Converters
{
    public class MathAddConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue && parameter != null && int.TryParse(parameter.ToString(), out int paramValue))
            {
                return intValue + paramValue;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
