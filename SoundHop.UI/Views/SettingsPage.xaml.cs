using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using SoundHop.UI.Services;
using SoundHop.UI.ViewModels;
using System.Reflection;

namespace SoundHop.UI.Views
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            
            // Initialize from settings
            RunAtStartupToggle.IsOn = SettingsService.Instance.RunAtStartup;
            StartMinimizedToggle.IsOn = SettingsService.Instance.StartMinimized;
            QuickSwitchToggle.IsOn = SettingsService.Instance.QuickSwitchMode;
            SyncCommunicationToggle.IsOn = SettingsService.Instance.SyncCommunicationDevice;
            ShowDisabledToggle.IsOn = SettingsService.Instance.ShowDisabledDevices;
            ShowDisconnectedToggle.IsOn = SettingsService.Instance.ShowDisconnectedDevices;
            AutoCheckUpdatesToggle.IsOn = SettingsService.Instance.AutoCheckUpdates;
            
            // Set version from assembly
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            VersionText.Text = $"Version {version?.Major}.{version?.Minor}.{version?.Build}";
        }

        private void RunAtStartupToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleSwitch ts)
            {
                SettingsService.Instance.RunAtStartup = ts.IsOn;
            }
        }

        private void StartMinimizedToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleSwitch ts)
            {
                SettingsService.Instance.StartMinimized = ts.IsOn;
            }
        }

        private void QuickSwitchToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleSwitch ts)
            {
                SettingsService.Instance.QuickSwitchMode = ts.IsOn;
            }
        }

        private void SyncCommunicationToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleSwitch ts)
            {
                SettingsService.Instance.SyncCommunicationDevice = ts.IsOn;
            }
        }

        private void ShowDisabledToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleSwitch ts)
            {
                SettingsService.Instance.ShowDisabledDevices = ts.IsOn;
                // Trigger device reload
                MainViewModel.Instance.LoadDevices();
            }
        }

        private void ShowDisconnectedToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleSwitch ts)
            {
                SettingsService.Instance.ShowDisconnectedDevices = ts.IsOn;
                // Trigger device reload
                MainViewModel.Instance.LoadDevices();
            }
        }

        private void AutoCheckUpdatesToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleSwitch ts)
            {
                SettingsService.Instance.AutoCheckUpdates = ts.IsOn;
            }
        }

        /// <summary>
        /// Returns "On" or "Off" text for toggle state display.
        /// </summary>
        public string GetToggleText(bool isOn) => isOn ? "On" : "Off";
    }
}

