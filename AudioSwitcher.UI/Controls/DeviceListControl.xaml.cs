using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using AudioSwitcher.UI.ViewModels;
using AudioSwitcher.Core.Models;
using AudioSwitcher.UI.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace AudioSwitcher.UI.Controls
{
    public sealed partial class DeviceListControl : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(MainViewModel), typeof(DeviceListControl),
                new PropertyMetadata(null, OnViewModelChanged));

        public static readonly DependencyProperty IsCompactProperty =
            DependencyProperty.Register(nameof(IsCompact), typeof(bool), typeof(DeviceListControl),
                new PropertyMetadata(false, OnIsCompactChanged));

        public MainViewModel ViewModel
        {
            get => (MainViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public bool IsCompact
        {
            get => (bool)GetValue(IsCompactProperty);
            set => SetValue(IsCompactProperty, value);
        }

        public DeviceListControl()
        {
            this.InitializeComponent();
            InitializeSortViewOptions();
        }

        private void InitializeSortViewOptions()
        {
            var settings = SettingsService.Instance;
            
            // Initialize sort mode checkmarks
            UpdateSortModeCheckmarks(settings.DeviceSortMode);
            
            // Initialize view option checkmarks
            ShowDisabledCheck.Visibility = settings.ShowDisabledDevices ? Visibility.Visible : Visibility.Collapsed;
            ShowDisconnectedCheck.Visibility = settings.ShowDisconnectedDevices ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateSortModeCheckmarks(string sortMode)
        {
            SortByFriendlyNameCheck.Visibility = sortMode == "FriendlyName" ? Visibility.Visible : Visibility.Collapsed;
            SortByDeviceNameCheck.Visibility = sortMode == "DeviceName" ? Visibility.Visible : Visibility.Collapsed;
        }

        private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DeviceListControl control && e.NewValue is MainViewModel vm)
            {
                control.UpdateDeviceLists();
                control.UpdateSelectedItem();
                
                // Subscribe to Devices collection changes
                vm.Devices.CollectionChanged += (s, args) => control.UpdateDeviceLists();
                
                vm.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == nameof(MainViewModel.DefaultDevice))
                    {
                        control.UpdateSelectedItem();
                    }
                };
            }
        }

        private void UpdateDeviceLists()
        {
            if (ViewModel == null) return;
            
            var favorites = ViewModel.Devices.Where(d => d.IsFavorite).ToList();
            var nonFavorites = ViewModel.Devices.Where(d => !d.IsFavorite).ToList();
            
            FavoritesList.ItemsSource = favorites;
            NonFavoritesList.ItemsSource = nonFavorites;
            
            // Show divider only when both sections have items
            FavoritesDivider.Visibility = favorites.Count > 0 && nonFavorites.Count > 0 
                ? Visibility.Visible 
                : Visibility.Collapsed;
            
            // Hide empty lists
            FavoritesList.Visibility = favorites.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            NonFavoritesList.Visibility = nonFavorites.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private static void OnIsCompactChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DeviceListControl control)
            {
                var padding = (bool)e.NewValue ? new Thickness(4) : new Thickness(0);
                control.FavoritesList.Padding = padding;
                control.NonFavoritesList.Padding = padding;
            }
        }

        private void UpdateSelectedItem()
        {
            if (ViewModel?.DefaultDevice == null) return;
            
            // Clear selection in both lists, set in the correct one
            if (ViewModel.DefaultDevice.IsFavorite)
            {
                FavoritesList.SelectedItem = ViewModel.DefaultDevice;
                NonFavoritesList.SelectedItem = null;
            }
            else
            {
                FavoritesList.SelectedItem = null;
                NonFavoritesList.SelectedItem = ViewModel.DefaultDevice;
            }
        }

        private void DeviceList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is AudioDevice device)
            {
                // Ignore clicks on disabled or disconnected devices
                if (!device.IsActive) return;

                if (ViewModel != null && device.Id != ViewModel.DefaultDevice?.Id)
                {
                    ViewModel.SetDefault(device);
                    UpdateDeviceLists(); // Refresh to update selection across lists
                }
            }
        }

        private void DeviceItem_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            // Show favorite button on row hover for non-favorites
            if (sender is Grid grid)
            {
                var favoriteButton = FindChildByName<Button>(grid, "FavoriteButton");
                if (favoriteButton != null)
                {
                    favoriteButton.Opacity = 1.0;
                }
            }
        }

        private void DeviceItem_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            // Hide favorite button when not hovering for non-favorites
            if (sender is Grid grid && grid.DataContext is AudioDevice device)
            {
                var favoriteButton = FindChildByName<Button>(grid, "FavoriteButton");
                if (favoriteButton != null && !device.IsFavorite)
                {
                    favoriteButton.Opacity = 0.0;
                }
            }
        }

        private T? FindChildByName<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            int childCount = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T element && element.Name == name)
                {
                    return element;
                }
                var result = FindChildByName<T>(child, name);
                if (result != null) return result;
            }
            return null;
        }

        private void ToggleFavorite_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is AudioDevice device && ViewModel != null)
            {
                ViewModel.ToggleFavorite(device);
                UpdateDeviceLists(); // Refresh lists after favorite change
            }
        }

        private async void SetHotkey_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item && item.Tag is AudioDevice device && ViewModel != null)
            {
                var dialog = new AudioSwitcher.UI.Dialogs.HotkeyEditorDialog(ViewModel.Devices, device);
                dialog.XamlRoot = this.XamlRoot;
                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary && dialog.ResultHotkey != null)
                {
                    device.HotKey = dialog.ResultHotkey;
                    
                    var settings = SettingsService.Instance;
                    var hotkeys = settings.Hotkeys;
                    if (hotkeys.ContainsKey(device.Id))
                    {
                        hotkeys[device.Id] = device.HotKey;
                    }
                    else
                    {
                        hotkeys.Add(device.Id, device.HotKey);
                    }
                    settings.Hotkeys = hotkeys;
                    
                    ViewModel.ReloadHotkeys();
                }
            }
        }

        private void SetDefaultCommunication_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item && item.Tag is AudioDevice device && ViewModel != null)
            {
                ViewModel.SetDefaultCommunicationDevice(device);
            }
        }

        private void ClearHotkey_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item && item.Tag is AudioDevice device && ViewModel != null)
            {
                device.HotKey = null;
                
                var settings = SettingsService.Instance;
                var hotkeys = settings.Hotkeys;
                if (hotkeys.ContainsKey(device.Id))
                {
                    hotkeys.Remove(device.Id);
                    settings.Hotkeys = hotkeys;
                }
                
                ViewModel.ReloadHotkeys();
            }
        }

        private void DisableDevice_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item && item.Tag is AudioDevice device && ViewModel != null)
            {
                ViewModel.DisableDevice(device);
            }
        }

        private void EnableDevice_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item && item.Tag is AudioDevice device && ViewModel != null)
            {
                ViewModel.EnableDevice(device);
            }
        }

        private void FavoriteButton_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (sender is Button button && button.Content is Grid grid)
            {
                var normalIcon = grid.Children.OfType<FontIcon>().FirstOrDefault(f => f.Name == "FavoriteIconNormal");
                var hoverIcon = grid.Children.OfType<FontIcon>().FirstOrDefault(f => f.Name == "FavoriteIconHover");
                if (normalIcon != null) normalIcon.Visibility = Visibility.Collapsed;
                if (hoverIcon != null) hoverIcon.Visibility = Visibility.Visible;
            }
        }

        private void FavoriteButton_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (sender is Button button && button.Content is Grid grid)
            {
                var normalIcon = grid.Children.OfType<FontIcon>().FirstOrDefault(f => f.Name == "FavoriteIconNormal");
                var hoverIcon = grid.Children.OfType<FontIcon>().FirstOrDefault(f => f.Name == "FavoriteIconHover");
                if (normalIcon != null) normalIcon.Visibility = Visibility.Visible;
                if (hoverIcon != null) hoverIcon.Visibility = Visibility.Collapsed;
            }
        }

        private async void ChangeIcon_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item && item.Tag is AudioDevice device && ViewModel != null)
            {
                System.Diagnostics.Debug.WriteLine($"[ChangeIcon] Opening dialog for device: {device.Name}");
                
                var dialog = new Dialogs.IconPickerDialog();
                dialog.XamlRoot = this.XamlRoot;
                
                // Pre-select current custom icon if set
                if (!string.IsNullOrEmpty(device.CustomIconGlyph))
                {
                    System.Diagnostics.Debug.WriteLine($"[ChangeIcon] Pre-selecting icon: {device.CustomIconGlyph}");
                    dialog.SelectIcon(device.CustomIconGlyph);
                }
                
                var result = await dialog.ShowAsync();
                
                System.Diagnostics.Debug.WriteLine($"[ChangeIcon] Dialog result: {result}, SelectedIconGlyph: {dialog.SelectedIconGlyph ?? "null"}");
                
                if (result == ContentDialogResult.Primary && !string.IsNullOrEmpty(dialog.SelectedIconGlyph))
                {
                    System.Diagnostics.Debug.WriteLine($"[ChangeIcon] Setting icon to: {dialog.SelectedIconGlyph}");
                    ViewModel.SetDeviceIcon(device, dialog.SelectedIconGlyph);
                }
                else if (result == ContentDialogResult.Secondary || dialog.ResetToDefault)
                {
                    System.Diagnostics.Debug.WriteLine($"[ChangeIcon] Resetting to default");
                    ViewModel.SetDeviceIcon(device, null); // Reset to default
                }
            }
        }

        // Sort/View Options Handlers
        private void SortByFriendlyName_Click(object sender, RoutedEventArgs e)
        {
            SettingsService.Instance.DeviceSortMode = "FriendlyName";
            UpdateSortModeCheckmarks("FriendlyName");
            ViewModel?.LoadDevices();
            UpdateDeviceLists();
        }

        private void SortByDeviceName_Click(object sender, RoutedEventArgs e)
        {
            SettingsService.Instance.DeviceSortMode = "DeviceName";
            UpdateSortModeCheckmarks("DeviceName");
            ViewModel?.LoadDevices();
            UpdateDeviceLists();
        }

        private void ShowDisabledToggle_Click(object sender, RoutedEventArgs e)
        {
            // Toggle the setting
            var newValue = !SettingsService.Instance.ShowDisabledDevices;
            SettingsService.Instance.ShowDisabledDevices = newValue;
            ShowDisabledCheck.Visibility = newValue ? Visibility.Visible : Visibility.Collapsed;
            ViewModel?.LoadDevices();
            UpdateDeviceLists();
        }

        private void ShowDisconnectedToggle_Click(object sender, RoutedEventArgs e)
        {
            // Toggle the setting
            var newValue = !SettingsService.Instance.ShowDisconnectedDevices;
            SettingsService.Instance.ShowDisconnectedDevices = newValue;
            ShowDisconnectedCheck.Visibility = newValue ? Visibility.Visible : Visibility.Collapsed;
            ViewModel?.LoadDevices();
            UpdateDeviceLists();
        }
    }
}


