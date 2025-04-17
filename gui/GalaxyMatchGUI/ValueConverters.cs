using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Data.Converters;

namespace GalaxyMatchGUI
{
    public class AttributeIconConverter : IValueConverter
    {
        private static readonly Dictionary<int, string> AttributeIcons = new Dictionary<int, string>
        {
            { 1, "🧠" },
            { 2, "💕" },
            { 3, "📏" } 
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int attributeId && AttributeIcons.TryGetValue(attributeId, out string icon))
            {
                return icon;
            }
            return "❓";
        } 

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AttributeNameConverter : IValueConverter
    {
        private static readonly Dictionary<int, string> AttributeNames = new Dictionary<int, string>
        {
            { 1, "Telepathic" },
            { 2, "3 Hearts" },
            { 3, "7.2 ft" }
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int attributeId && AttributeNames.TryGetValue(attributeId, out string name))
            {
                return name;
            }
            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
