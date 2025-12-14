using Microsoft.UI.Xaml.Controls;
using AudioSwitcher.UI.ViewModels;
using AudioSwitcher.Core.Models;
using AudioSwitcher.UI.Services;

namespace AudioSwitcher.UI.Views
{
    public sealed partial class FlyoutPage : Page
    {
        public MainViewModel ViewModel { get; }

        public FlyoutPage()
        {
            this.InitializeComponent();
            ViewModel = MainViewModel.Instance;
            ViewModel.Initialize();
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            DeviceListControl.ViewModel = ViewModel;
            
            // Initialize sort/view options
            InitializeSortViewOptions();
            
            // Set initial tray icon from default device
            this.Loaded += (s, e) =>
            {
                if (ViewModel.DefaultDevice != null)
                {
                    UpdateTrayIcon(ViewModel.DefaultDevice);
                }
            };
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

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.DefaultDevice) && ViewModel.DefaultDevice != null)
            {
               UpdateTrayIcon(ViewModel.DefaultDevice);
            }
        }

        private void UpdateTrayIcon(AudioSwitcher.Core.Models.AudioDevice device)
        {
            string glyph = device.DisplayIcon;
            string tooltip = device.Name;
            
            System.Diagnostics.Trace.WriteLine($"FlyoutPage.UpdateTrayIcon: glyph='{glyph}', tooltip='{tooltip}'");
            
            if (Microsoft.UI.Xaml.Application.Current is App app && app.Window is FlyoutWindow flyoutWindow)
            {
                System.Diagnostics.Trace.WriteLine($"FlyoutPage.UpdateTrayIcon: TrayManager is {(flyoutWindow.TrayManager != null ? "not null" : "NULL")}");
                flyoutWindow.UpdateIconFromFluentGlyph(glyph, tooltip);
            }
            else
            {
                System.Diagnostics.Trace.WriteLine("FlyoutPage.UpdateTrayIcon: Could not access MainWindow");
            }
        }

        private void OpenDashboard_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (Microsoft.UI.Xaml.Application.Current is App app && app.Window is FlyoutWindow flyoutWindow)
            {
                flyoutWindow.OpenDashboard();
            }
        }

        private void DeviceTypeSelector_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            DeviceListControl.IsInputMode = sender.SelectedItem == InputsTab;
        }

        // Sort/View Options Handlers
        private void SortByFriendlyName_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            SettingsService.Instance.DeviceSortMode = "FriendlyName";
            UpdateSortModeCheckmarks("FriendlyName");
            ViewModel.LoadDevices();
        }

        private void SortByDeviceName_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            SettingsService.Instance.DeviceSortMode = "DeviceName";
            UpdateSortModeCheckmarks("DeviceName");
            ViewModel.LoadDevices();
        }

        private void ShowDisabledToggle_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var newValue = !SettingsService.Instance.ShowDisabledDevices;
            SettingsService.Instance.ShowDisabledDevices = newValue;
            ShowDisabledCheck.Visibility = newValue ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
            ViewModel.LoadDevices();
        }

        private void ShowDisconnectedToggle_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var newValue = !SettingsService.Instance.ShowDisconnectedDevices;
            SettingsService.Instance.ShowDisconnectedDevices = newValue;
            ShowDisconnectedCheck.Visibility = newValue ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
            ViewModel.LoadDevices();
        }
    }
}
