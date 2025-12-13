using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using Microsoft.UI.Dispatching;
using AudioSwitcher.Core.Models;
using AudioSwitcher.Core.Services;

namespace AudioSwitcher.UI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private static MainViewModel? _instance;
        private static readonly object _lock = new object();
        
        public static MainViewModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new MainViewModel();
                    }
                }
                return _instance;
            }
        }

        private readonly AudioDeviceService _service;
        private readonly SettingsService _settingsService;
        private DispatcherQueue? _dispatcherQueue;
        private ObservableCollection<AudioDevice> _devices;

        private MainViewModel()
        {
            _service = new AudioDeviceService();
            _settingsService = SettingsService.Instance;
            _devices = new ObservableCollection<AudioDevice>();
        }

        /// <summary>
        /// Initialize the ViewModel on the UI thread. Must be called from the main dispatcher thread.
        /// </summary>
        public void Initialize()
        {
            if (_dispatcherQueue != null) return; // Already initialized
            
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            _service.DevicesChanged += () => _dispatcherQueue?.TryEnqueue(LoadDevices);
            LoadDevices();
        }

        public ObservableCollection<AudioDevice> Devices
        {
            get => _devices;
            set { _devices = value; OnPropertyChanged(); }
        }

        private ObservableCollection<AudioDevice> _configuredHotkeys = new ObservableCollection<AudioDevice>();
        public ObservableCollection<AudioDevice> ConfiguredHotkeys
        {
            get => _configuredHotkeys;
            set { _configuredHotkeys = value; OnPropertyChanged(); }
        }

        private ObservableCollection<AudioSwitcher.Core.Models.HotkeyDisplayInfo> _allHotkeyDisplayInfos = new();
        /// <summary>
        /// All hotkeys including disconnected devices.
        /// </summary>
        public ObservableCollection<AudioSwitcher.Core.Models.HotkeyDisplayInfo> AllHotkeyDisplayInfos
        {
            get => _allHotkeyDisplayInfos;
            set { _allHotkeyDisplayInfos = value; OnPropertyChanged(); }
        }

        private AudioDevice? _defaultDevice;
        public AudioDevice? DefaultDevice
        {
            get => _defaultDevice;
            set { _defaultDevice = value; OnPropertyChanged(); }
        }

        public void LoadDevices()
        {
            // Build device state filter from settings
            var stateFilter = AudioSwitcher.Core.Com.DeviceState.Active;
            if (_settingsService.Settings.ShowDisabledDevices)
                stateFilter |= AudioSwitcher.Core.Com.DeviceState.Disabled;
            if (_settingsService.Settings.ShowDisconnectedDevices)
                stateFilter |= AudioSwitcher.Core.Com.DeviceState.Unplugged; // Only Unplugged, not NotPresent (matches Windows Sound settings)

            var newDevices = _service.GetPlaybackDevices(stateFilter);
            var favorites = _settingsService.Settings.FavoriteDeviceIds;
            var hotkeys = _settingsService.Settings.Hotkeys;
            var customIcons = _settingsService.Settings.CustomDeviceIcons;

            // Check for device ID migrations (device name matches but ID changed)
            foreach (var d in newDevices)
            {
                // Check if this device's name matches an old ID in the name mapping
                string? oldId = _settingsService.FindOldIdByName(d.DisplayName, d.DisplaySubName, d.Id);
                if (oldId != null)
                {
                    // Migrate settings from old ID to new ID
                    _settingsService.MigrateDeviceSettings(oldId, d.Id, d.DisplayName, d.DisplaySubName);
                    
                    // Refresh the local references after migration
                    favorites = _settingsService.Settings.FavoriteDeviceIds;
                    hotkeys = _settingsService.Settings.Hotkeys;
                    customIcons = _settingsService.Settings.CustomDeviceIcons;
                }
                else
                {
                    // Update the name mapping for this device
                    _settingsService.UpdateDeviceNameMapping(d.Id, d.DisplayName, d.DisplaySubName);
                }
            }

            // Apply favorites, hotkeys, and custom icons to new list
            foreach (var d in newDevices)
            {
                d.IsFavorite = favorites.Contains(d.Id);
                if (hotkeys.TryGetValue(d.Id, out var hotkey))
                {
                    d.HotKey = hotkey;
                }
                if (customIcons.TryGetValue(d.Id, out var iconGlyph))
                {
                    d.CustomIconGlyph = iconGlyph;
                }
            }

            // Sync: Remove missing
            for (int i = Devices.Count - 1; i >= 0; i--)
            {
                if (!newDevices.Exists(d => d.Id == Devices[i].Id))
                {
                    Devices.RemoveAt(i);
                }
            }
            
            // Sync: Update / Add
            foreach (var newDev in newDevices)
            {
                var existing = Devices.FirstOrDefault(d => d.Id == newDev.Id);
                if (existing != null)
                {
                    existing.Name = newDev.Name;
                    existing.IsDefault = newDev.IsDefault;
                    existing.IsDefaultComms = newDev.IsDefaultComms;
                    existing.IconPath = newDev.IconPath;
                    existing.IsFavorite = newDev.IsFavorite;
                    existing.HotKey = newDev.HotKey;
                    existing.State = newDev.State; // Sync enabled/disabled state
                    existing.CustomIconGlyph = newDev.CustomIconGlyph; // Sync custom icon
                }
                else
                {
                    Devices.Add(newDev);
                }
            }

            // Retrieve updated list for sorting (Devices collection is mixed order now)
            var sortedList = Devices.ToList();
            sortedList.Sort((a, b) => 
            {
                if (a.IsFavorite != b.IsFavorite) return b.IsFavorite.CompareTo(a.IsFavorite);
                return string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
            });

            // Reorder if necessary (bubble sort / move)
            for (int i = 0; i < sortedList.Count; i++)
            {
                var item = sortedList[i];
                var oldIndex = Devices.IndexOf(item);
                if (oldIndex != i)
                {
                   Devices.Move(oldIndex, i);
                }
            }

            // Set divider visibility - first non-favorite after favorites
            bool hasFavorites = Devices.Any(d => d.IsFavorite);
            bool passedFavorites = false;
            foreach (var device in Devices)
            {
                if (hasFavorites && !device.IsFavorite && !passedFavorites)
                {
                    device.ShowDividerAbove = true;
                    passedFavorites = true;
                }
                else
                {
                    device.ShowDividerAbove = false;
                }
            }
            
            DefaultDevice = Devices.FirstOrDefault(d => d.IsDefault);
            ReloadHotkeys();
        }
        
        public void ReloadHotkeys()
        {
            var hotkeyService = AudioSwitcher.UI.Services.GlobalHotkeyService.Instance;
            hotkeyService.UnregisterAll();
            ConfiguredHotkeys.Clear();
            AllHotkeyDisplayInfos.Clear();

            var connectedDeviceIds = new HashSet<string>(Devices.Select(d => d.Id));

            // Add hotkeys for connected devices
            foreach (var d in Devices)
            {
                if (d.HotKey != null)
                {
                    ConfiguredHotkeys.Add(d);
                    AllHotkeyDisplayInfos.Add(new AudioSwitcher.Core.Models.HotkeyDisplayInfo
                    {
                        DeviceId = d.Id,
                        DisplayName = d.DisplayName,
                        SubName = d.DisplaySubName,
                        Hotkey = d.HotKey,
                        IsConnected = true,
                        Device = d
                    });
                    hotkeyService.Register(d.HotKey.Modifiers, d.HotKey.Key, () => 
                    {
                        var target = Devices.FirstOrDefault(x => x.Id == d.Id);
                        if (target != null)
                        {
                            _dispatcherQueue.TryEnqueue(() => SetDefault(target));
                        }
                    });
                }
            }

            // Add hotkeys for disconnected devices (from settings)
            var settingsHotkeys = _settingsService.Settings.Hotkeys;
            var nameMapping = _settingsService.Settings.DeviceNameMapping;
            
            foreach (var kvp in settingsHotkeys)
            {
                if (!connectedDeviceIds.Contains(kvp.Key))
                {
                    // Disconnected device - get name info from mapping
                    var nameInfo = nameMapping.TryGetValue(kvp.Key, out var info) ? info : null;
                    AllHotkeyDisplayInfos.Add(new AudioSwitcher.Core.Models.HotkeyDisplayInfo
                    {
                        DeviceId = kvp.Key,
                        DisplayName = nameInfo?.DisplayName ?? "Unknown Device",
                        SubName = nameInfo?.SubName ?? "",
                        Hotkey = kvp.Value,
                        IsConnected = false,
                        Device = null
                    });
                }
            }
        }
        
        public void ToggleFavorite(AudioDevice device)
        {
            if (device == null) return;
            
            device.IsFavorite = !device.IsFavorite;
            
            if (device.IsFavorite)
            {
                if (!_settingsService.Settings.FavoriteDeviceIds.Contains(device.Id))
                    _settingsService.Settings.FavoriteDeviceIds.Add(device.Id);
            }
            else
            {
                _settingsService.Settings.FavoriteDeviceIds.Remove(device.Id);
            }
            
            _settingsService.Save();
            LoadDevices(); // Re-sort
        }
        
        public void SetDefault(AudioDevice device)
        {
            if (device == null || !device.IsActive) return;
            _service.SetDefaultDevice(device.Id);
            
            // Update UI state without full reload to preserve scroll position
            foreach (var d in Devices)
            {
                d.IsDefault = (d.Id == device.Id);
            }
            DefaultDevice = device;

            // Sync communication device if setting enabled
            if (_settingsService.Settings.SyncCommunicationDevice)
            {
                SetDefaultCommunicationDevice(device);
            }
        }

        public void SetDefaultCommunicationDevice(AudioDevice device)
        {
            if (device == null || !device.IsActive) return;
            _service.SetDefaultCommunicationDevice(device.Id);
            
            // Update IsDefaultComms state
            foreach (var d in Devices)
            {
                d.IsDefaultComms = (d.Id == device.Id);
            }
        }

        /// <summary>
        /// Cycles to the next favorite device. Used for quick switch mode.
        /// </summary>
        public void CycleToNextFavorite()
        {
            var favorites = Devices.Where(d => d.IsFavorite && d.IsActive).ToList();
            if (favorites.Count == 0) return;

            // Find current default in favorites list
            int currentIndex = favorites.FindIndex(d => d.Id == DefaultDevice?.Id);
            
            // Move to next favorite (wrap around)
            int nextIndex = (currentIndex + 1) % favorites.Count;
            SetDefault(favorites[nextIndex]);
        }

        /// <summary>
        /// Disables an audio device.
        /// </summary>
        public void DisableDevice(AudioDevice device)
        {
            if (device == null || !device.IsActive) return;
            _service.SetDeviceEnabled(device.Id, false);
            LoadDevices(); // Reload to reflect the change
        }

        /// <summary>
        /// Enables a disabled audio device.
        /// </summary>
        public void EnableDevice(AudioDevice device)
        {
            if (device == null || !device.IsDisabled) return;
            _service.SetDeviceEnabled(device.Id, true);
            LoadDevices(); // Reload to reflect the change
        }

        /// <summary>
        /// Sets or clears a custom icon for the device.
        /// </summary>
        public void SetDeviceIcon(AudioDevice device, string? iconGlyph)
        {
            if (device == null) return;
            
            System.Diagnostics.Debug.WriteLine($"[SetDeviceIcon] Device: {device.Id}, IconGlyph length: {iconGlyph?.Length ?? 0}");
            
            if (string.IsNullOrEmpty(iconGlyph))
            {
                // Remove custom icon
                _settingsService.Settings.CustomDeviceIcons.Remove(device.Id);
                System.Diagnostics.Debug.WriteLine($"[SetDeviceIcon] Removed custom icon");
            }
            else
            {
                // Set custom icon
                _settingsService.Settings.CustomDeviceIcons[device.Id] = iconGlyph;
                System.Diagnostics.Debug.WriteLine($"[SetDeviceIcon] Set custom icon, dict count: {_settingsService.Settings.CustomDeviceIcons.Count}");
            }
            
            _settingsService.Save();
            System.Diagnostics.Debug.WriteLine($"[SetDeviceIcon] Saved settings");
            
            // Refresh devices to update icons in UI
            LoadDevices();
            System.Diagnostics.Debug.WriteLine($"[SetDeviceIcon] Reloaded devices");
            
            // If this is the default device, notify to update tray icon
            if (device.IsDefault)
            {
                OnPropertyChanged(nameof(DefaultDevice));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
