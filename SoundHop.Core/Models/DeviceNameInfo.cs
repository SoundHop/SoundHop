namespace SoundHop.Core.Models
{
    /// <summary>
    /// Stores device identification info for settings persistence.
    /// Used to match devices when their ID changes (e.g., USB devices).
    /// </summary>
    public class DeviceNameInfo
    {
        /// <summary>
        /// The customizable display name (first line, e.g., "Speakers", "MAIN Left/Right")
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;
        
        /// <summary>
        /// The sub/device name (second line, e.g., "Qudelix-5K USB DAC 96KHz", "Minifuse 1")
        /// </summary>
        public string SubName { get; set; } = string.Empty;
    }
}
