using System;
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
        private readonly AudioDeviceService _service;
        private readonly SettingsService _settingsService;
        private readonly DispatcherQueue _dispatcherQueue;
        private ObservableCollection<AudioDevice> _devices;

        public MainViewModel()
        {
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            _service = new AudioDeviceService();
            _service.DevicesChanged += () => _dispatcherQueue.TryEnqueue(LoadDevices);
            
            _settingsService = new SettingsService();
            _devices = new ObservableCollection<AudioDevice>();
            LoadDevices();
        }

        public ObservableCollection<AudioDevice> Devices
        {
            get => _devices;
            set { _devices = value; OnPropertyChanged(); }
        }

        private AudioDevice? _defaultDevice;
        public AudioDevice? DefaultDevice
        {
            get => _defaultDevice;
            set { _defaultDevice = value; OnPropertyChanged(); }
        }

        public void LoadDevices()
        {
            var newDevices = _service.GetPlaybackDevices();
            var favorites = _settingsService.Settings.FavoriteDeviceIds;

            // Apply favorites to new list (needed for sorting logic)
            foreach (var d in newDevices)
            {
                d.IsFavorite = favorites.Contains(d.Id);
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
                    existing.IconPath = newDev.IconPath;
                    existing.IsFavorite = newDev.IsFavorite;
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
            
            DefaultDevice = Devices.FirstOrDefault(d => d.IsDefault);
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
            if (device == null) return;
            _service.SetDefaultDevice(device.Id);
            
            // Update UI state without full reload to preserve scroll position
            foreach (var d in Devices)
            {
                d.IsDefault = (d.Id == device.Id);
            }
            DefaultDevice = device;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
