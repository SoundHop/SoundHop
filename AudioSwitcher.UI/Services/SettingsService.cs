using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Win32;

namespace AudioSwitcher.UI.Services
{
    public class SettingsService
    {
        private static SettingsService? _instance;
        public static SettingsService Instance => _instance ??= new SettingsService();

        private readonly string _settingsFilePath;
        private Dictionary<string, object> _settings;
        
        private const string RunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "AudioSwitcher";

        public event EventHandler<string>? SettingChanged;

        private SettingsService()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(appData, "AudioSwitcher");
            Directory.CreateDirectory(appFolder);
            _settingsFilePath = Path.Combine(appFolder, "settings.json");
            _settings = LoadSettings();
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
            get => GetValue(nameof(ShowTrayIcon), true);
            set
            {
                SetValue(nameof(ShowTrayIcon), value);
                SettingChanged?.Invoke(this, nameof(ShowTrayIcon));
            }
        }

        public bool MinimizeToTray
        {
            get => GetValue(nameof(MinimizeToTray), false);
            set
            {
                SetValue(nameof(MinimizeToTray), value);
                SettingChanged?.Invoke(this, nameof(MinimizeToTray));
            }
        }

        public bool CloseToTray
        {
            get => GetValue(nameof(CloseToTray), true);
            set
            {
                SetValue(nameof(CloseToTray), value);
                SettingChanged?.Invoke(this, nameof(CloseToTray));
            }
        }
        
        private T GetValue<T>(string key, T defaultValue)
        {
            if (_settings.TryGetValue(key, out var value))
            {
                if (value is JsonElement element)
                {
                    if (typeof(T) == typeof(bool)) return (T)(object)element.GetBoolean();
                    if (typeof(T) == typeof(string)) return (T)(object)element.GetString()!;
                    if (typeof(T) == typeof(int)) return (T)(object)element.GetInt32();
                }
                return (T)value;
            }
            return defaultValue;
        }

        private void SetValue<T>(string key, T value)
        {
            _settings[key] = value!;
            SaveSettings();
        }

        private Dictionary<string, object> LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    var json = File.ReadAllText(_settingsFilePath);
                    return JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"Failed to load settings: {ex}");
            }
            return new Dictionary<string, object>();
        }

        private void SaveSettings()
        {
            try
            {
                var json = JsonSerializer.Serialize(_settings);
                File.WriteAllText(_settingsFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"Failed to save settings: {ex}");
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
