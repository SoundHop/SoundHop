using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace SoundHop.UI.Converters
{
    /// <summary>
    /// Converts a bool to visibility: true = Collapsed, false = Visible (inverse of normal)
    /// </summary>
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool b)
            {
                return b ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
