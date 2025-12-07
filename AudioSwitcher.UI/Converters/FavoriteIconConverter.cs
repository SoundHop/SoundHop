using System;
using Microsoft.UI.Xaml.Data;

namespace AudioSwitcher.UI.Converters
{
    public class FavoriteIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isFavorite && isFavorite)
            {
                return "\uE735"; // Solid Star or Pin
            }
            return "\uE734"; // Outline Star
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
