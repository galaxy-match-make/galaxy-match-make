using Avalonia;
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
            {
                return new SolidColorBrush(Color.Parse("#4C1D95")); // Default purple color
            }

            var currentInterest = values[0] as Interest;
            var selectedInterests = values[1] as ObservableCollection<Interest>;

            if (currentInterest == null || selectedInterests == null)
            {
                return new SolidColorBrush(Color.Parse("#4C1D95")); // Default purple color
            }

            // Check if the current interest is selected
            if (selectedInterests.Any(i => i.Id == currentInterest.Id))
            {
                // Selected - use a bright color
                return new SolidColorBrush(Color.Parse("#FF00FF")); // Bright pink color
            }
            else
            {
                // Not selected - use a darker, subdued color
                return new SolidColorBrush(Color.Parse("#4C1D95")); // Purple color
            }
        }
    }
}