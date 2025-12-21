using Microsoft.UI.Xaml.Controls;
using SoundHop.UI.ViewModels;
using SoundHop.UI.Services;

namespace SoundHop.UI.Views
{
    public sealed partial class OutputDevicesPage : Page
    {
        public OutputDevicesPage()
        {
            this.InitializeComponent();
            DeviceListControl.ViewModel = MainViewModel.Instance;
            InitializeSortViewOptions();
        }

        private void InitializeSortViewOptions()
        {
            var settings = SettingsService.Instance;
            UpdateSortModeCheckmarks(settings.DeviceSortMode);
            ShowDisabledCheck.Visibility = settings.ShowDisabledDevices ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
            ShowDisconnectedCheck.Visibility = settings.ShowDisconnectedDevices ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
        }

        private void UpdateSortModeCheckmarks(string sortMode)
        {
            SortByFriendlyNameCheck.Visibility = sortMode == "FriendlyName" ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
            SortByDeviceNameCheck.Visibility = sortMode == "DeviceName" ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
        }

        private void SortByFriendlyName_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            SettingsService.Instance.DeviceSortMode = "FriendlyName";
            UpdateSortModeCheckmarks("FriendlyName");
            MainViewModel.Instance.LoadDevices();
        }

        private void SortByDeviceName_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            SettingsService.Instance.DeviceSortMode = "DeviceName";
            UpdateSortModeCheckmarks("DeviceName");
            MainViewModel.Instance.LoadDevices();
        }

        private void ShowDisabledToggle_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var newValue = !SettingsService.Instance.ShowDisabledDevices;
            SettingsService.Instance.ShowDisabledDevices = newValue;
            ShowDisabledCheck.Visibility = newValue ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
            MainViewModel.Instance.LoadDevices();
        }

        private void ShowDisconnectedToggle_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var newValue = !SettingsService.Instance.ShowDisconnectedDevices;
            SettingsService.Instance.ShowDisconnectedDevices = newValue;
            ShowDisconnectedCheck.Visibility = newValue ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
            MainViewModel.Instance.LoadDevices();
        }
    }
}
