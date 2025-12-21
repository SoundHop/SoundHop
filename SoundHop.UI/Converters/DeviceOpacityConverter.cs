using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace SoundHop.UI.Converters
{
    /// <summary>
    /// Returns opacity value based on device active state.
    /// Inactive devices (disabled or disconnected) are dimmed to 0.5 opacity.
    /// </summary>
    public class DeviceOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isActive)
            {
                return isActive ? 1.0 : 0.5;
            }
            return 1.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

