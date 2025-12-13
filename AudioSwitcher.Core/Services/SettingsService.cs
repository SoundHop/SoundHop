using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace AudioSwitcher.Core.Services
{
    public class SettingsService
    {
        private static SettingsService? _instance;
        public static SettingsService Instance => _instance ??= new SettingsService();

        private static readonly string SettingsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AudioSwitcher", "settings.json");
        
        public class AppSettings
        {
            public List<string> FavoriteDeviceIds { get; set; } = new List<string>();
            public Dictionary<string, AudioSwitcher.Core.Models.Hotkey> Hotkeys { get; set; } = new Dictionary<string, AudioSwitcher.Core.Models.Hotkey>();
            
            // UI Settings
            public bool ShowTrayIcon { get; set; } = true;
            public bool MinimizeToTray { get; set; } = false;
            public bool CloseToTray { get; set; } = true;
            public bool StartMinimized { get; set; } = false;
            
            // Device Switching
            public bool QuickSwitchMode { get; set; } = false;
            public bool SyncCommunicationDevice { get; set; } = true;
            
            // Device Display
            public bool ShowDisabledDevices { get; set; } = false;
            public bool ShowDisconnectedDevices { get; set; } = false;
            
            // Custom Device Icons (device ID -> Fluent icon glyph)
            public Dictionary<string, string> CustomDeviceIcons { get; set; } = new Dictionary<string, string>();
            
            // Device Name Mapping (device ID -> DeviceNameInfo) for ID migration
            public Dictionary<string, AudioSwitcher.Core.Models.DeviceNameInfo> DeviceNameMapping { get; set; } = new Dictionary<string, AudioSwitcher.Core.Models.DeviceNameInfo>();
        }

        public AppSettings Settings { get; private set; }

        private SettingsService()
        {
            Load();
        }

        public void Load()
        {
            try
            {
                if (File.Exists(SettingsFile))
                {
                    string json = File.ReadAllText(SettingsFile);
                    Settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
                else
                {
                    Settings = new AppSettings();
                }
            }
            catch
            {
                Settings = new AppSettings();
            }
        }

        public void Save()
        {
            try
            {
                string dir = Path.GetDirectoryName(SettingsFile);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                string json = JsonSerializer.Serialize(Settings);
                File.WriteAllText(SettingsFile, json);
            }
            catch { }
        }

        /// <summary>
        /// Finds an old device ID that matches the given device name info.
        /// Used for migrating settings when a device reconnects with a new ID.
        /// Matches by SubName (hardware name) since DisplayName can be changed by user in Windows.
        /// </summary>
        public string? FindOldIdByName(string displayName, string subName, string currentId)
        {
            foreach (var kvp in Settings.DeviceNameMapping)
            {
                // Skip the current ID
                if (kvp.Key == currentId) continue;
                
                // Match by SubName (hardware name) which is more stable
                // Also check DisplayName matches to avoid matching wrong device with same hardware
                if (kvp.Value.SubName == subName && kvp.Value.DisplayName == displayName)
                {
                    return kvp.Key;
                }
            }
            return null;
        }

        /// <summary>
        /// Migrates all settings from an old device ID to a new one.
        /// </summary>
        public void MigrateDeviceSettings(string oldId, string newId, string displayName, string subName)
        {
            bool changed = false;

            // Migrate favorites
            if (Settings.FavoriteDeviceIds.Contains(oldId))
            {
                Settings.FavoriteDeviceIds.Remove(oldId);
                if (!Settings.FavoriteDeviceIds.Contains(newId))
                    Settings.FavoriteDeviceIds.Add(newId);
                changed = true;
            }

            // Migrate hotkeys
            if (Settings.Hotkeys.TryGetValue(oldId, out var hotkey))
            {
                Settings.Hotkeys.Remove(oldId);
                Settings.Hotkeys[newId] = hotkey;
                changed = true;
            }

            // Migrate custom icons
            if (Settings.CustomDeviceIcons.TryGetValue(oldId, out var icon))
            {
                Settings.CustomDeviceIcons.Remove(oldId);
                Settings.CustomDeviceIcons[newId] = icon;
                changed = true;
            }

            // Update name mapping - remove old, add new
            Settings.DeviceNameMapping.Remove(oldId);
            Settings.DeviceNameMapping[newId] = new AudioSwitcher.Core.Models.DeviceNameInfo
            {
                DisplayName = displayName,
                SubName = subName
            };

            if (changed)
            {
                System.Diagnostics.Debug.WriteLine($"[SettingsService] Migrated device settings from '{oldId}' to '{newId}' for '{displayName} ({subName})'");
                Save();
            }
        }

        /// <summary>
        /// Updates the device name mapping for a device.
        /// </summary>
        public void UpdateDeviceNameMapping(string deviceId, string displayName, string subName)
        {
            if (Settings.DeviceNameMapping.TryGetValue(deviceId, out var existing) && 
                existing.DisplayName == displayName && existing.SubName == subName)
                return; // No change needed
            
            Settings.DeviceNameMapping[deviceId] = new AudioSwitcher.Core.Models.DeviceNameInfo
            {
                DisplayName = displayName,
                SubName = subName
            };
            Save();
        }

        /// <summary>
        /// Gets the device name info for a device ID, or null if not found.
        /// </summary>
        public AudioSwitcher.Core.Models.DeviceNameInfo? GetDeviceNameInfo(string deviceId)
        {
            return Settings.DeviceNameMapping.TryGetValue(deviceId, out var info) ? info : null;
        }
    }
}
