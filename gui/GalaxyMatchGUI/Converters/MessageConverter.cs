using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;
using Avalonia.Layout;
using Avalonia;

namespace GalaxyMatchGUI.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string colorParams)
            {
                var colors = colorParams.Split('|');
                if (colors.Length == 2)
                {
                    var colorStr = boolValue ? colors[0] : colors[1];
                    return SolidColorBrush.Parse(colorStr);
                }
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string alignmentParams)
            {
                var alignments = alignmentParams.Split('|');
                if (alignments.Length == 2)
                {
                    var alignmentStr = boolValue ? alignments[0] : alignments[1];
                    return Enum.Parse<HorizontalAlignment>(alignmentStr);
                }
            }
            return HorizontalAlignment.Left;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public class BoolToThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string thicknessParams)
            {
                var thicknesses = thicknessParams.Split('|');
                if (thicknesses.Length == 2)
                {
                    var thicknessStr = boolValue ? thicknesses[0] : thicknesses[1];
                    return Thickness.Parse(thicknessStr);
                }
            }
            return new Thickness(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}