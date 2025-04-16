using System;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;

namespace GalaxyMatchGUI.Converters
{
    public class InitialsConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string name && !string.IsNullOrWhiteSpace(name))
            {
                var nameParts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (nameParts.Length > 0)
                {
                    if (nameParts.Length == 1)
                    {
                        return nameParts[0].Substring(0, 1).ToUpper();
                    }
                    else
                    {
                        return $"{nameParts[0].Substring(0, 1)}{nameParts[nameParts.Length - 1].Substring(0, 1)}".ToUpper();
                    }
                }
            }
            
            return "?";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}