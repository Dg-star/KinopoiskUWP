using System;
using Windows.UI.Xaml.Data;

namespace KinopoiskUWP.Converters
{
    public class RatingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || (value is double rating && rating == 0))
            {
                return "—";
            }
            return string.Format(GlobalizationHelper.InvariantCulture, "{0:F1}", value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public static class GlobalizationHelper
    {
        public static IFormatProvider InvariantCulture { get; } =
            System.Globalization.CultureInfo.InvariantCulture;
    }
}