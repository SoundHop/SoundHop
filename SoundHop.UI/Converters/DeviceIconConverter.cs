using System;
using SoundHop.Core.Models;
using Microsoft.UI.Xaml.Data;

namespace SoundHop.UI.Converters
{
    public class DeviceIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // Handle both binding patterns:
            // 1. Old: value is AudioDevice (for backwards compat)
            // 2. New: value is CustomIconGlyph (string?), parameter is AudioDevice
            AudioDevice? device = null;
            
            if (value is AudioDevice dev)
            {
                device = dev;
            }
            else if (parameter is AudioDevice paramDev)
            {
                device = paramDev;
            }
            
            if (device == null) return "\uE7F4";

            // Priority 1: User-set custom icon in app
            if (!string.IsNullOrEmpty(device.CustomIconGlyph))
            {
                return device.CustomIconGlyph;
            }

            // Priority 2: Try parse from Windows IconPath
            // e.g. @%SystemRoot%\System32\mmres.dll,-3010
            if (!string.IsNullOrEmpty(device.IconPath))
            {
                // Check if it's a non-default custom icon (not from mmres.dll)
                // If the user set a custom icon via Sound control panel, it might point elsewhere
                if (!device.IconPath.Contains("mmres.dll", StringComparison.OrdinalIgnoreCase))
                {
                    // Non-default icon - use a generic audio icon since we can't load external icons
                    return "\uE767"; // Music note as fallback for custom system icons
                }

                try
                {
                    var parts = device.IconPath.Split(',');
                    if (parts.Length == 2 && int.TryParse(parts[1], out int id))
                    {
                        int absId = Math.Abs(id);
                        return absId switch
                        {
                            3010 => "\uE7F5", // Speakers
                            3011 => "\uE7F6", // Headphones
                            3012 => "\uE7F6", // Headset?
                            3013 => "\uE7F3", // Digital/SPDIF -> Server/Generic look? or E7F3
                            3014 => "\uE7F5", // Line Out -> Speakers
                            3015 => "\uE7F4", // Monitor
                            3016 => "\uE7F5", // Speakers
                            3017 => "\uE7F5", // Speakers
                            3018 => "\uE7F5", // Speakers
                            3019 => "\uE7F5", // Speakers
                            3020 => "\uE7F5", // Speakers
                            3021 => "\uE7F5", // Speakers
                            3030 => "\uE720", // Microphone
                            3031 => "\uE720", // Microphone
                            _ => "\uE7F4"     // Default Monitor
                        };
                    }
                }
                catch {}
            }

            // Fallback heuristic based on name
            var name = device.Name.ToLowerInvariant();
            if (name.Contains("headphone") || name.Contains("headset")) return "\uE7F6"; 
            if (name.Contains("speaker")) return "\uE7F5";
            
            return "\uE7F4"; // Monitor
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
