using System;
using Microsoft.UI.Xaml.Data;

namespace SoundHop.UI.Converters
{
    /// <summary>
    /// Converts a bool to opacity: true = 1.0 (fully visible), false = 0.5 (grayed)
    /// </summary>
    public class BoolToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool b)
            {
                return b ? 1.0 : 0.5;
            }
            return 1.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
