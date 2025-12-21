using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Win32;

namespace SoundHop.UI.Services
{
    public class SettingsService
    {
        private static SettingsService? _instance;
        public static SettingsService Instance => _instance ??= new SettingsService();

        private const string RunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "SoundHop";
        private readonly SoundHop.Core.Services.SettingsService _coreSettings = SoundHop.Core.Services.SettingsService.Instance;

        public event EventHandler<string>? SettingChanged;

        private SettingsService()
        {
        }

        public bool RunAtStartup
        {
            get => IsAppRunAtStartup();
            set
            {
                SetAppRunAtStartup(value);
                SettingChanged?.Invoke(this, nameof(RunAtStartup));
            }
        }

        public bool ShowTrayIcon
        {
            get => _coreSettings.Settings.ShowTrayIcon;
            set
            {
                _coreSettings.Settings.ShowTrayIcon = value;
                _coreSettings.Save();
                SettingChanged?.Invoke(this, nameof(ShowTrayIcon));
            }
        }

        public bool MinimizeToTray
        {
            get => _coreSettings.Settings.MinimizeToTray;
            set
            {
                _coreSettings.Settings.MinimizeToTray = value;
                _coreSettings.Save();
                SettingChanged?.Invoke(this, nameof(MinimizeToTray));
            }
        }

        public bool CloseToTray
        {
            get => _coreSettings.Settings.CloseToTray;
            set
            {
                _coreSettings.Settings.CloseToTray = value;
                _coreSettings.Save();
                SettingChanged?.Invoke(this, nameof(CloseToTray));
            }
        }

        public bool StartMinimized
        {
            get => _coreSettings.Settings.StartMinimized;
            set
            {
                _coreSettings.Settings.StartMinimized = value;
                _coreSettings.Save();
                SettingChanged?.Invoke(this, nameof(StartMinimized));
            }
        }
        
        public Dictionary<string, SoundHop.Core.Models.Hotkey> Hotkeys
        {
            get => _coreSettings.Settings.Hotkeys;
            set
            {
                _coreSettings.Settings.Hotkeys = value;
                _coreSettings.Save();
                SettingChanged?.Invoke(this, nameof(Hotkeys));
            }
        }

        public bool QuickSwitchMode
        {
            get => _coreSettings.Settings.QuickSwitchMode;
            set
            {
                _coreSettings.Settings.QuickSwitchMode = value;
                _coreSettings.Save();
                SettingChanged?.Invoke(this, nameof(QuickSwitchMode));
            }
        }

        public bool SyncCommunicationDevice
        {
            get => _coreSettings.Settings.SyncCommunicationDevice;
            set
            {
                _coreSettings.Settings.SyncCommunicationDevice = value;
                _coreSettings.Save();
                SettingChanged?.Invoke(this, nameof(SyncCommunicationDevice));
            }
        }

        public bool ShowDisabledDevices
        {
            get => _coreSettings.Settings.ShowDisabledDevices;
            set
            {
                _coreSettings.Settings.ShowDisabledDevices = value;
                _coreSettings.Save();
                SettingChanged?.Invoke(this, nameof(ShowDisabledDevices));
            }
        }

        public bool ShowDisconnectedDevices
        {
            get => _coreSettings.Settings.ShowDisconnectedDevices;
            set
            {
                _coreSettings.Settings.ShowDisconnectedDevices = value;
                _coreSettings.Save();
                SettingChanged?.Invoke(this, nameof(ShowDisconnectedDevices));
            }
        }

        public string DeviceSortMode
        {
            get => _coreSettings.Settings.DeviceSortMode;
            set
            {
                _coreSettings.Settings.DeviceSortMode = value;
                _coreSettings.Save();
                SettingChanged?.Invoke(this, nameof(DeviceSortMode));
            }
        }

        private bool IsAppRunAtStartup()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunKey, false);
                return key?.GetValue(AppName) != null;
            }
            catch
            {
                return false;
            }
        }

        private void SetAppRunAtStartup(bool enable)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunKey, true);
                if (key == null) return;

                if (enable)
                {
                    string? path = Environment.ProcessPath;
                    if (!string.IsNullOrEmpty(path))
                    {
                        key.SetValue(AppName, path);
                    }
                }
                else
                {
                    key.DeleteValue(AppName, false);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"Failed to set startup: {ex.Message}");
            }
        }
    }
}
