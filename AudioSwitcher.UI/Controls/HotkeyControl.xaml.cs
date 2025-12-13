using Microsoft.UI.Xaml.Controls;

namespace AudioSwitcher.UI.Controls
{
    public sealed partial class HotkeyControl : UserControl
    {
        public HotkeyControl()
        {
            this.InitializeComponent();
        }

        public AudioSwitcher.Core.Models.Hotkey? Hotkey
        {
            get => (AudioSwitcher.Core.Models.Hotkey?)GetValue(HotkeyProperty);
            set => SetValue(HotkeyProperty, value);
        }

        public static readonly Microsoft.UI.Xaml.DependencyProperty HotkeyProperty =
            Microsoft.UI.Xaml.DependencyProperty.Register("Hotkey", typeof(AudioSwitcher.Core.Models.Hotkey), typeof(HotkeyControl), new Microsoft.UI.Xaml.PropertyMetadata(null, OnHotkeyChanged));
            
        private static void OnHotkeyChanged(Microsoft.UI.Xaml.DependencyObject d, Microsoft.UI.Xaml.DependencyPropertyChangedEventArgs e)
        {
            if (d is HotkeyControl control)
            {
                control.Render();
            }
        }

        private void Render()
        {
            KeysPanel.Children.Clear();
            if (Hotkey == null) return;
            
            var parts = new System.Collections.Generic.List<string>();
            if (Hotkey.Modifiers.HasFlag(AudioSwitcher.Core.Models.KeyModifiers.Windows)) parts.Add("Win");
            if (Hotkey.Modifiers.HasFlag(AudioSwitcher.Core.Models.KeyModifiers.Control)) parts.Add("Ctrl");
            if (Hotkey.Modifiers.HasFlag(AudioSwitcher.Core.Models.KeyModifiers.Shift)) parts.Add("Shift");
            if (Hotkey.Modifiers.HasFlag(AudioSwitcher.Core.Models.KeyModifiers.Alt)) parts.Add("Alt");
            
            string keyName = ((Windows.System.VirtualKey)Hotkey.Key).ToString();
            // Clean up key names if needed (e.g. Number1 -> 1)
            if (keyName.StartsWith("Number")) keyName = keyName.Substring(6);
            parts.Add(keyName);

            foreach (var part in parts)
            {
                var border = new Border { Style = (Microsoft.UI.Xaml.Style)Resources["HotkeyKeyStyle"] };
                border.Child = new TextBlock { Text = part.ToUpper(), Style = (Microsoft.UI.Xaml.Style)Resources["HotkeyTextStyle"] };
                KeysPanel.Children.Add(border);
            }
        }
    }
}
