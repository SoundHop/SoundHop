using System;
using Microsoft.UI.Xaml.Data;

namespace AudioSwitcher.UI.Converters
{
    /// <summary>
    /// Converts a bool to opacity for hiding: true = 1.0 (visible), false = 0.0 (hidden, but still takes space)
    /// Used for favorite icons that should only show on hover for non-favorites
    /// </summary>
    public class BoolToOpacityHiddenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool b)
            {
                return b ? 1.0 : 0.0;
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
