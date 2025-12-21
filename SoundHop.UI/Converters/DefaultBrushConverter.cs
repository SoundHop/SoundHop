using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;

namespace SoundHop.UI.Converters
{
    public class DefaultBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isDefault && isDefault)
            {
                // Return a subtle accent color highlight
                // Use system resource or explicit color. 
                // SystemAccentColor with low opacity is standard for selection.
                // But for simplicity, let's return a predefined resource compatible brush, or a hardcoded semi-transparent white/grey for "active" look.
                // Assuming Dark mode mostly (based on screenshot).
                // Let's use a SolidColorBrush with 0.1 opacity white/accent.
                var brush = new SolidColorBrush(Microsoft.UI.Colors.White);
                brush.Opacity = 0.1; 
                return brush;
            }
            return new SolidColorBrush(Microsoft.UI.Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
