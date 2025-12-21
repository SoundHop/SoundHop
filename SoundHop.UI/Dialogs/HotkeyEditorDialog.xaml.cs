using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SoundHop.Core.Models;
using Windows.System;
using System;
using System.Linq;

namespace SoundHop.UI.Dialogs
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
        private readonly List<AudioDevice> _outputDevices;
        private readonly List<AudioDevice> _inputDevices;
        private bool _isInitialized = false;

        /// <summary>
        /// Constructor with separate output and input device lists
        /// </summary>
        public HotkeyEditorDialog(
            IEnumerable<AudioDevice> outputDevices, 
            IEnumerable<AudioDevice> inputDevices, 
            AudioDevice? preSelectedDevice = null,
            bool? selectInputs = null)
        {
            this.InitializeComponent();
            _outputDevices = outputDevices.OrderBy(d => d.Name).ToList();
            _inputDevices = inputDevices.OrderBy(d => d.Name).ToList();
            
            // If a device is pre-selected, determine its type and lock selection
            if (preSelectedDevice != null)
            {
                if (preSelectedDevice.IsInput)
                {
                    InputsRadio.IsChecked = true;
                    LoadDevices(isInput: true);
                }
                else
                {
                    OutputsRadio.IsChecked = true;
                    LoadDevices(isInput: false);
                }
                DeviceComboBox.SelectedItem = preSelectedDevice;
                DeviceComboBox.IsEnabled = false;
                DeviceTypeSelector.IsEnabled = false;
            }
            else
            {
                // If selectInputs is specified, use that, otherwise default to outputs
                if (selectInputs == true)
                {
                    InputsRadio.IsChecked = true;
                    LoadDevices(isInput: true);
                }
                else
                {
                    OutputsRadio.IsChecked = true;
                    LoadDevices(isInput: false);
                }
            }
            
            _isInitialized = true;
        }

        /// <summary>
        /// Constructor for editing hotkey of a disconnected device (read-only device name)
        /// </summary>
        public HotkeyEditorDialog(Hotkey? existingHotkey, string deviceName)
        {
            this.InitializeComponent();
            _outputDevices = new List<AudioDevice>();
            _inputDevices = new List<AudioDevice>();
            
            // Hide device type selector and combo, show device name as text
            DeviceTypeSelector.Visibility = Visibility.Collapsed;
            DeviceComboBox.Visibility = Visibility.Collapsed;
            
            // Add device name text to the parent StackPanel
            var stackPanel = DeviceComboBox.Parent as StackPanel;
            if (stackPanel != null)
            {
                var deviceNameText = new TextBlock
                {
                    Text = deviceName + " (Disconnected)",
                    Style = (Style)Application.Current.Resources["BodyTextBlockStyle"],
                    Margin = new Thickness(0, 0, 0, 16)
                };
                // Insert at position 1 (after the hidden device type selector)
                stackPanel.Children.Insert(1, deviceNameText);
            }
            
            // Pre-fill existing hotkey if any
            if (existingHotkey != null)
            {
                _currentModifiers = existingHotkey.Modifiers;
                _currentKey = existingHotkey.Key;
                ResultHotkey = existingHotkey;
                UpdatePreview();
            }
            
            _isInitialized = true;
        }

        private void DeviceTypeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized) return;
            
            bool isInput = InputsRadio.IsChecked == true;
            LoadDevices(isInput);
        }

        private void LoadDevices(bool isInput)
        {
            var devices = isInput ? _inputDevices : _outputDevices;
            DeviceComboBox.ItemsSource = devices;
            if (devices.Any())
            {
                DeviceComboBox.SelectedIndex = 0;
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
            var allDevices = _outputDevices.Concat(_inputDevices);
            var conflictingDevice = allDevices.FirstOrDefault(d => 
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

