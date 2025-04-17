using Avalonia.Data.Converters;
using Avalonia.Media;
using GalaxyMatchGUI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace GalaxyMatchGUI.Converters
{
    public class InterestSelectedConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count < 2 || values[0] == null || values[1] == null)
                return new SolidColorBrush(Color.Parse("#4C1D95")); // Default background

            var interest = values[0] as Interest;
            var selectedInterests = values[1] as ObservableCollection<Interest>;

            if (interest != null && selectedInterests != null)
            {
                bool isSelected = selectedInterests.Any(i => i.Id == interest.Id);
                return isSelected 
                    ? new SolidColorBrush(Color.Parse("#FF00FF"))
                    : new SolidColorBrush(Color.Parse("#4C1D95"));
            }

            return new SolidColorBrush(Color.Parse("#4C1D95"));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}