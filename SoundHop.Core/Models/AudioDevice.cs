using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SoundHop.Core.Models
{
    public class AudioDevice : INotifyPropertyChanged
    {
        private bool _isDefault;
        private bool _isFavorite;
        private bool _isDefaultComms;
        private Hotkey? _hotKey;
        private bool _showDividerAbove;

        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        
        public bool IsDefault 
        { 
            get => _isDefault; 
            set { if (_isDefault != value) { _isDefault = value; OnPropertyChanged(); } } 
        }
        
        public string IconPath { get; set; } = string.Empty;
        
        private string? _customIconGlyph;
        /// <summary>
        /// User-selected custom Fluent Icon glyph. Takes priority over IconPath.
        /// </summary>
        public string? CustomIconGlyph 
        { 
            get => _customIconGlyph; 
            set 
            { 
                if (_customIconGlyph != value) 
                { 
                    _customIconGlyph = value; 
                    OnPropertyChanged(); 
                    OnPropertyChanged(nameof(DisplayIcon)); // Also update computed icon
                } 
            } 
        }
        
        /// <summary>
        /// Gets the icon glyph to display. Returns CustomIconGlyph if set, otherwise calculates from IconPath.
        /// </summary>
        public string DisplayIcon
        {
            get
            {
                // Priority 1: Custom icon
                if (!string.IsNullOrEmpty(CustomIconGlyph))
                    return CustomIconGlyph;
                
                // Priority 2: Parse from IconPath
                if (!string.IsNullOrEmpty(IconPath))
                {
                    // Check if non-default (custom system icon)
                    if (!IconPath.Contains("mmres.dll", System.StringComparison.OrdinalIgnoreCase))
                        return "\uE767"; // Music note fallback
                    
                    try
                    {
                        var parts = IconPath.Split(',');
                        if (parts.Length == 2 && int.TryParse(parts[1], out int id))
                        {
                            int absId = System.Math.Abs(id);
                            return absId switch
                            {
                                3010 => "\uE7F5", // Speakers
                                3011 => "\uE7F6", // Headphones
                                3012 => "\uE7F6", // Headset
                                3013 => "\uE7F3", // Digital/SPDIF
                                3014 => "\uE7F5", // Line Out
                                3015 => "\uE7F4", // Monitor
                                3030 => "\uE720", // Microphone
                                3031 => "\uE720", // Microphone
                                _ => "\uE7F4"
                            };
                        }
                    }
                    catch { }
                }
                
                // Fallback: name-based heuristic
                var name = Name.ToLowerInvariant();
                if (name.Contains("headphone") || name.Contains("headset")) return "\uE7F6";
                if (name.Contains("speaker")) return "\uE7F5";
                
                return "\uE7F4"; // Default Monitor
            }
        } 
        
        public bool IsFavorite 
        { 
            get => _isFavorite; 
            set { if (_isFavorite != value) { _isFavorite = value; OnPropertyChanged(); } } 
        } 
        
        public Hotkey? HotKey
        {
            get => _hotKey;
            set { if (_hotKey != value) { _hotKey = value; OnPropertyChanged(); } }
        } 
          
        public bool IsDefaultComms 
        { 
            get => _isDefaultComms; 
            set { if (_isDefaultComms != value) { _isDefaultComms = value; OnPropertyChanged(); } } 
        }

        private uint _state = 1;
        public uint State 
        { 
            get => _state; 
            set 
            { 
                if (_state != value) 
                { 
                    _state = value; 
                    OnPropertyChanged(); 
                    OnPropertyChanged(nameof(IsDisabled));
                    OnPropertyChanged(nameof(IsDisconnected));
                    OnPropertyChanged(nameof(IsActive));
                } 
            } 
        }
        
        public bool IsDisabled => State == 2; // DeviceState.Disabled
        public bool IsDisconnected => State == 8; // DeviceState.Unplugged only
        public bool IsActive => State == 1; // DeviceState.Active
        
        private bool _isInput;
        /// <summary>
        /// True if this is an input/capture device (microphone), false for output/playback device (speaker).
        /// </summary>
        public bool IsInput 
        { 
            get => _isInput; 
            set { if (_isInput != value) { _isInput = value; OnPropertyChanged(); } } 
        }
        
        public string DisplayName 
        { 
            get 
            {
                var match = System.Text.RegularExpressions.Regex.Match(Name, @"^(.*)\s\((.*)\)$");
                if (match.Success) return match.Groups[1].Value;
                return Name;
            } 
        }

        public string DisplaySubName 
        { 
            get 
            {
                var match = System.Text.RegularExpressions.Regex.Match(Name, @"^(.*)\s\((.*)\)$");
                if (match.Success) return match.Groups[2].Value;
                return string.Empty;
            } 
        }

        public bool HasSubName => !string.IsNullOrEmpty(DisplaySubName);

        public bool ShowDividerAbove 
        { 
            get => _showDividerAbove; 
            set { if (_showDividerAbove != value) { _showDividerAbove = value; OnPropertyChanged(); } } 
        }
        
        public override string ToString() => Name;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
