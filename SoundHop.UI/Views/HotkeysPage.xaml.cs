using Microsoft.UI.Xaml.Controls;
using SoundHop.UI.ViewModels;
using SoundHop.Core.Models;
using System;
using System.Linq;

namespace SoundHop.UI.Views
{
    public sealed partial class HotkeysPage : Page
    {
        public MainViewModel ViewModel => (MainViewModel)DataContext;

        public HotkeysPage()
        {
            this.InitializeComponent();
            var vm = MainViewModel.Instance;
            vm.Initialize();
            DataContext = vm;
        }

        private async void AddHotkey_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var dialog = new SoundHop.UI.Dialogs.HotkeyEditorDialog(
                ViewModel.Devices, 
                ViewModel.InputDevices);
            dialog.XamlRoot = this.XamlRoot;
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary && dialog.ResultHotkey != null && dialog.SelectedDevice != null)
            {
                UpdateHotkeyByDeviceId(dialog.SelectedDevice.Id, dialog.ResultHotkey);
            }
        }

        private async void Edit_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is HotkeyDisplayInfo info)
            {
                if (info.IsConnected && info.Device != null)
                {
                    // Connected device - allow full editing with device selection
                    var dialog = new SoundHop.UI.Dialogs.HotkeyEditorDialog(
                        ViewModel.Devices, 
                        ViewModel.InputDevices, 
                        info.Device);
                    dialog.XamlRoot = this.XamlRoot;
                    var result = await dialog.ShowAsync();
                    if (result == ContentDialogResult.Primary && dialog.ResultHotkey != null)
                    {
                        UpdateHotkeyByDeviceId(info.DeviceId, dialog.ResultHotkey);
                    }
                }
                else
                {
                    // Disconnected device - only allow hotkey editing, not device change
                    var dialog = new SoundHop.UI.Dialogs.HotkeyEditorDialog(info.Hotkey, info.DisplayName);
                    dialog.XamlRoot = this.XamlRoot;
                    var result = await dialog.ShowAsync();
                    if (result == ContentDialogResult.Primary && dialog.ResultHotkey != null)
                    {
                        UpdateHotkeyByDeviceId(info.DeviceId, dialog.ResultHotkey);
                    }
                }
            }
        }

        private void Delete_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is HotkeyDisplayInfo info)
            {
                RemoveHotkeyByDeviceId(info.DeviceId);
            }
        }

        private void UpdateHotkeyByDeviceId(string deviceId, Hotkey hotkey)
        {
            var settings = Services.SettingsService.Instance;
            var hotkeys = settings.Hotkeys;
            hotkeys[deviceId] = hotkey;
            settings.Hotkeys = hotkeys; // Trigger save
            
            // Also update the device if connected
            var device = ViewModel.Devices.FirstOrDefault(d => d.Id == deviceId);
            if (device != null)
            {
                device.HotKey = hotkey;
            }
            
            // Reload to update registration
            ViewModel.ReloadHotkeys();
        }

        private void RemoveHotkeyByDeviceId(string deviceId)
        {
            var settings = Services.SettingsService.Instance;
            var hotkeys = settings.Hotkeys;
            if (hotkeys.ContainsKey(deviceId))
            {
                hotkeys.Remove(deviceId);
                settings.Hotkeys = hotkeys; // Trigger save
            }
            
            // Also clear on device if connected
            var device = ViewModel.Devices.FirstOrDefault(d => d.Id == deviceId);
            if (device != null)
            {
                device.HotKey = null;
            }
            
            ViewModel.ReloadHotkeys();
        }
    }
}
