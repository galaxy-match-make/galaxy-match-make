using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace GalaxyMatchGUI.Converters
{
    public class BoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string options)
            {
                var parts = options.Split('|');
                if (parts.Length == 2)
                {
                    return boolValue ? parts[0] : parts[1];
                }
            }
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }

    public static class BoolConverters
    {
        public static readonly IValueConverter ToString = new BoolToStringConverter();
    }
}