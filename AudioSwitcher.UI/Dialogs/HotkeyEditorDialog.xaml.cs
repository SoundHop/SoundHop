using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AudioSwitcher.Core.Models;
using Windows.System;
using System;
using System.Linq;

namespace AudioSwitcher.UI.Dialogs
{
    public sealed partial class HotkeyEditorDialog : ContentDialog
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        public AudioDevice? SelectedDevice => DeviceComboBox.SelectedItem as AudioDevice;
        public Hotkey? ResultHotkey { get; private set; }
        
        private DispatcherTimer? _pollTimer;
        private bool _isCapturing;
        private KeyModifiers _currentModifiers;
        private int _currentKey;
        private readonly List<AudioDevice> _allDevices;

        public HotkeyEditorDialog(IEnumerable<AudioDevice> devices, AudioDevice? preSelectedDevice = null)
        {
            this.InitializeComponent();
            _allDevices = devices.ToList();
            DeviceComboBox.ItemsSource = _allDevices;
            
            if (preSelectedDevice != null)
            {
                DeviceComboBox.SelectedItem = preSelectedDevice;
                DeviceComboBox.IsEnabled = false;
            }
            else
            {
                DeviceComboBox.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Constructor for editing hotkey of a disconnected device (read-only device name)
        /// </summary>
        public HotkeyEditorDialog(Hotkey? existingHotkey, string deviceName)
        {
            this.InitializeComponent();
            _allDevices = new List<AudioDevice>();
            
            // Hide device combo and show device name as text
            DeviceComboBox.Visibility = Visibility.Collapsed;
            
            // Display device name as read-only text
            var deviceNameText = new TextBlock
            {
                Text = deviceName + " (Disconnected)",
                Style = (Style)Application.Current.Resources["BodyTextBlockStyle"],
                VerticalAlignment = VerticalAlignment.Center
            };
            
            // Find the parent grid and add the text
            if (DeviceComboBox.Parent is Grid parentGrid)
            {
                Grid.SetRow(deviceNameText, 0);
                Grid.SetColumn(deviceNameText, 1);
                parentGrid.Children.Add(deviceNameText);
            }
            
            // Pre-fill existing hotkey if any
            if (existingHotkey != null)
            {
                _currentModifiers = existingHotkey.Modifiers;
                _currentKey = existingHotkey.Key;
                ResultHotkey = existingHotkey;
                UpdatePreview();
            }
        }

        private void CaptureButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isCapturing)
            {
                StopCapture();
            }
            else
            {
                StartCapture();
            }
        }

        private void StartCapture()
        {
            _isCapturing = true;
            _currentKey = 0;
            _currentModifiers = KeyModifiers.None;
            
            StatusText.Text = "Listening for keys... Press any key combination.";
            PlaceholderText.Text = "Press keys now...";
            PreviewHotkeyControl.Visibility = Visibility.Collapsed;
            PlaceholderText.Visibility = Visibility.Visible;
            ErrorText.Visibility = Visibility.Collapsed;
            
            _pollTimer = new DispatcherTimer();
            _pollTimer.Interval = TimeSpan.FromMilliseconds(16); // ~60fps polling
            _pollTimer.Tick += PollKeyboard;
            _pollTimer.Start();
        }

        private void StopCapture()
        {
            _isCapturing = false;
            _pollTimer?.Stop();
            _pollTimer = null;
            
            if (_currentKey != 0)
            {
                StatusText.Text = $"Captured: {GetKeyName(_currentKey)}";
            }
            else
            {
                StatusText.Text = "";
                PlaceholderText.Text = "Click here, then press keys...";
            }
        }

        private void PollKeyboard(object? sender, object e)
        {
            // Check modifiers
            var modifiers = KeyModifiers.None;
            if (IsKeyDown(0x11)) modifiers |= KeyModifiers.Control; // VK_CONTROL
            if (IsKeyDown(0x10)) modifiers |= KeyModifiers.Shift;   // VK_SHIFT
            if (IsKeyDown(0x12)) modifiers |= KeyModifiers.Alt;     // VK_MENU
            if (IsKeyDown(0x5B) || IsKeyDown(0x5C)) modifiers |= KeyModifiers.Windows; // VK_LWIN/VK_RWIN

            _currentModifiers = modifiers;

            // Scan for pressed keys (excluding modifiers)
            // Check common key ranges
            for (int vk = 0x08; vk <= 0xFE; vk++)
            {
                // Skip modifier keys
                if (vk == 0x10 || vk == 0x11 || vk == 0x12 || // Shift, Ctrl, Alt
                    vk == 0x5B || vk == 0x5C || // Win keys
                    vk == 0xA0 || vk == 0xA1 || vk == 0xA2 || vk == 0xA3 || vk == 0xA4 || vk == 0xA5) // L/R variants
                {
                    continue;
                }

                if (IsKeyDown(vk))
                {
                    _currentKey = vk;
                    UpdatePreview();
                    StopCapture();
                    return;
                }
            }
        }

        private bool IsKeyDown(int vk)
        {
            return (GetAsyncKeyState(vk) & 0x8000) != 0;
        }

        private string GetKeyName(int vk)
        {
            // Try to get enum name first
            if (Enum.IsDefined(typeof(VirtualKey), vk))
            {
                return ((VirtualKey)vk).ToString();
            }
            
            // Fallback for extended keys
            return vk switch
            {
                0x7F => "F16",
                0x80 => "F17",
                0x81 => "F18",
                0x82 => "F19",
                0x83 => "F20",
                0x84 => "F21",
                0x85 => "F22",
                0x86 => "F23",
                0x87 => "F24",
                _ => $"Key{vk:X2}"
            };
        }

        private void UpdatePreview()
        {
            if (_currentKey == 0) return;

            ResultHotkey = new Hotkey
            {
                Modifiers = _currentModifiers,
                Key = _currentKey
            };

            PlaceholderText.Visibility = Visibility.Collapsed;
            PreviewHotkeyControl.Hotkey = ResultHotkey;
            PreviewHotkeyControl.Visibility = Visibility.Visible;

            // Check for duplicate hotkey on other devices
            CheckForDuplicateHotkey();
        }

        private void CheckForDuplicateHotkey()
        {
            if (ResultHotkey == null) return;

            var currentDevice = SelectedDevice;
            var conflictingDevice = _allDevices.FirstOrDefault(d => 
                d.HotKey != null && 
                d.Id != currentDevice?.Id &&
                d.HotKey.Modifiers == ResultHotkey.Modifiers && 
                d.HotKey.Key == ResultHotkey.Key);

            if (conflictingDevice != null)
            {
                ErrorText.Text = $"This hotkey is already assigned to \"{conflictingDevice.DisplayName}\"";
                ErrorText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Orange);
                ErrorText.Visibility = Visibility.Visible;
                IsPrimaryButtonEnabled = false;
            }
            else
            {
                ErrorText.Visibility = Visibility.Collapsed;
                IsPrimaryButtonEnabled = true;
            }
        }
    }
}

