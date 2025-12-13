namespace AudioSwitcher.Core.Models
{
    /// <summary>
    /// Represents a hotkey configuration for a device that may or may not be currently connected.
    /// Used for displaying hotkeys in the settings page, including disconnected devices.
    /// </summary>
    public class HotkeyDisplayInfo
    {
        public string DeviceId { get; set; } = string.Empty;
        public Hotkey? Hotkey { get; set; }
        public bool IsConnected { get; set; }
        public AudioDevice? Device { get; set; }
        
        /// <summary>
        /// The customizable display name (first line, e.g., "Speakers", "MAIN Left/Right")
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;
        
        /// <summary>
        /// The sub/device name (second line, e.g., "Qudelix-5K USB DAC 96KHz", "Minifuse 1")
        /// </summary>
        public string SubName { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether to show the sub name line (when SubName is not empty).
        /// </summary>
        public bool HasSubName => !string.IsNullOrEmpty(SubName);
    }
}
