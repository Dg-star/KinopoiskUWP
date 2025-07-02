using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace KinopoiskUWP.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolValue)
            {
                // Обработка параметра для инверсии (например, parameter="invert")
                bool invert = parameter?.ToString().ToLower() == "invert";

                if (invert)
                    boolValue = !boolValue;

                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Visibility visibility)
            {
                bool invert = parameter?.ToString().ToLower() == "invert";
                bool result = visibility == Visibility.Visible;

                return invert ? !result : result;
            }
            return false;
        }
    }
}