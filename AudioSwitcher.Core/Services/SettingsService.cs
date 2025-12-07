using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace AudioSwitcher.Core.Services
{
    public class SettingsService
    {
        private static readonly string SettingsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AudioSwitcher", "settings.json");
        
        public class AppSettings
        {
            public List<string> FavoriteDeviceIds { get; set; } = new List<string>();
        }

        public AppSettings Settings { get; private set; }

        public SettingsService()
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
    }
}
