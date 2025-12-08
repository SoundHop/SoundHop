using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using AudioSwitcher.UI.Services;

namespace AudioSwitcher.UI.Views
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            
            // Initialize from settings
            RunAtStartupToggle.IsOn = SettingsService.Instance.RunAtStartup;
            ShowTrayIconToggle.IsOn = SettingsService.Instance.ShowTrayIcon;
            MinimizeToTrayToggle.IsOn = SettingsService.Instance.MinimizeToTray;
            CloseToTrayToggle.IsOn = SettingsService.Instance.CloseToTray;
        }

        private void RunAtStartupToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleSwitch ts)
            {
                SettingsService.Instance.RunAtStartup = ts.IsOn;
            }
        }

        private void ShowTrayIconToggle_Toggled(object sender, RoutedEventArgs e)
        {
             if (sender is ToggleSwitch ts)
            {
                SettingsService.Instance.ShowTrayIcon = ts.IsOn;
            }
        }

        private void MinimizeToTrayToggle_Toggled(object sender, RoutedEventArgs e)
        {
             if (sender is ToggleSwitch ts)
            {
                SettingsService.Instance.MinimizeToTray = ts.IsOn;
            }
        }

        private void CloseToTrayToggle_Toggled(object sender, RoutedEventArgs e)
        {
             if (sender is ToggleSwitch ts)
            {
                SettingsService.Instance.CloseToTray = ts.IsOn;
            }
        }
    }
}
