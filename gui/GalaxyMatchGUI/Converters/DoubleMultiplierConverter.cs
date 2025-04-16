using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace GalaxyMatchGUI.Converters
{
    public class DoubleMultiplierConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double doubleValue && parameter is string multiplierString)
            {
                if (double.TryParse(multiplierString, out double multiplier))
                {
                    return doubleValue * multiplier;
                }
            }
            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}