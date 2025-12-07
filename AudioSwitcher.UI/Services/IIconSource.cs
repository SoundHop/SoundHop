using System;

namespace AudioSwitcher.UI.Services
{
    /// <summary>
    /// Interface for dynamic icon sources that can react to theme changes.
    /// Based on EarTrumpet's IShellNotifyIconSource pattern.
    /// </summary>
    public interface IIconSource
    {
        /// <summary>
        /// Gets the current icon handle (HICON).
        /// </summary>
        IntPtr Current { get; }
        
        /// <summary>
        /// Raised when the icon has changed (e.g., due to theme change).
        /// </summary>
        event Action<IIconSource>? Changed;
        
        /// <summary>
        /// Forces a check for whether the icon needs to be updated.
        /// Called periodically and on system events like display changes.
        /// </summary>
        void CheckForUpdate();
    }
}
