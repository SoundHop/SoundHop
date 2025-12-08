using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using AudioSwitcher.UI.ViewModels;
using AudioSwitcher.Core.Models;

namespace AudioSwitcher.UI.Views
{
    public sealed partial class FlyoutPage : Page
    {
        public MainViewModel ViewModel { get; }

        public FlyoutPage()
        {
            this.InitializeComponent();
            ViewModel = new MainViewModel();
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            
            // Set initial tray icon from default device
            this.Loaded += (s, e) =>
            {
                if (ViewModel.DefaultDevice != null)
                {
                    UpdateTrayIcon(ViewModel.DefaultDevice.IconPath, ViewModel.DefaultDevice.Name);
                }
            };
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.DefaultDevice) && ViewModel.DefaultDevice != null)
            {
               UpdateTrayIcon(ViewModel.DefaultDevice.IconPath, ViewModel.DefaultDevice.Name);
            }
        }

        private void UpdateTrayIcon(string iconPath, string tooltip)
        {
            System.Diagnostics.Trace.WriteLine($"MainPage.UpdateTrayIcon: iconPath='{iconPath}', tooltip='{tooltip}'");
            
            if (Microsoft.UI.Xaml.Application.Current is App app && app.Window is FlyoutWindow flyoutWindow)
            {
                System.Diagnostics.Trace.WriteLine($"FlyoutPage.UpdateTrayIcon: TrayManager is {(flyoutWindow.TrayManager != null ? "not null" : "NULL")}");
                
                // Convert icon path to Fluent glyph (same logic as DeviceIconConverter)
                string glyph = GetFluentGlyphForIconPath(iconPath);
                flyoutWindow.UpdateIconFromFluentGlyph(glyph, tooltip);
            }
            else
            {
                System.Diagnostics.Trace.WriteLine("MainPage.UpdateTrayIcon: Could not access MainWindow");
            }
        }

        private static string GetFluentGlyphForIconPath(string iconPath)
        {
            // Parse the icon path (e.g., "@%SystemRoot%\System32\mmres.dll,-3010")
            if (!string.IsNullOrEmpty(iconPath))
            {
                try
                {
                    var parts = iconPath.Split(',');
                    if (parts.Length == 2 && int.TryParse(parts[1], out int id))
                    {
                        int absId = System.Math.Abs(id);
                        return absId switch
                        {
                            3010 => "\uE7F5", // Speakers
                            3011 => "\uE7F6", // Headphones
                            3012 => "\uE7F6", // Headset
                            3013 => "\uE7F3", // Digital/SPDIF
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
                catch { }
            }
            return "\uE7F5"; // Default: Speakers
        }

        private void DeviceList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is AudioDevice device)
            {
                ViewModel.SetDefault(device);
            }
        }

        private void ToggleFavorite_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is AudioDevice device)
            {
                ViewModel.ToggleFavorite(device);
            }
        }

        private void DeviceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Mirror the item click behavior: when user selects an item, set it as default
            if (sender is ListView lv && lv.SelectedItem is AudioDevice device)
            {
                ViewModel.SetDefault(device);
            }
            else if (e.AddedItems != null && e.AddedItems.Count > 0 && e.AddedItems[0] is AudioDevice addedDevice)
            {
                ViewModel.SetDefault(addedDevice);
            }
        }
    }
}
