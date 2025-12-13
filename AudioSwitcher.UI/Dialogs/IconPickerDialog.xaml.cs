using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;

namespace AudioSwitcher.UI.Dialogs
{
    public sealed partial class IconPickerDialog : ContentDialog
    {
        public class IconItem
        {
            public string Glyph { get; set; } = "";
            public string Name { get; set; } = "";
        }

        public List<IconItem> Icons { get; } = new List<IconItem>
        {
            // Audio Output
            new IconItem { Glyph = "\uE7F5", Name = "Speaker" },
            new IconItem { Glyph = "\uE7F6", Name = "Headphones" },
            new IconItem { Glyph = "\uEC88", Name = "Headset" },
            new IconItem { Glyph = "\uE7F3", Name = "Digital/SPDIF" },
            new IconItem { Glyph = "\uE7F4", Name = "Monitor" },
            
            // TV & Displays
            new IconItem { Glyph = "\uE8B2", Name = "TV" },
            new IconItem { Glyph = "\uE7F8", Name = "Display" },
            new IconItem { Glyph = "\uE8A1", Name = "Projector" },
            
            // Mobile & Phones
            new IconItem { Glyph = "\uE8EA", Name = "Phone" },
            new IconItem { Glyph = "\uE8C7", Name = "Cell Phone" },
            new IconItem { Glyph = "\uE8F1", Name = "Tablet" },
            
            // Audio Input
            new IconItem { Glyph = "\uE720", Name = "Microphone" },
            new IconItem { Glyph = "\uE1D6", Name = "Webcam" },
            
            // Audio/Music
            new IconItem { Glyph = "\uE767", Name = "Music" },
            new IconItem { Glyph = "\uE8D6", Name = "Audio" },
            new IconItem { Glyph = "\uE995", Name = "Cast" },
            
            // Gaming & VR
            new IconItem { Glyph = "\uE990", Name = "Xbox" },
            new IconItem { Glyph = "\uE7FC", Name = "Gaming" },
            new IconItem { Glyph = "\uE8B8", Name = "VR Headset" },
            
            // Misc
            new IconItem { Glyph = "\uE703", Name = "Bluetooth" },
            new IconItem { Glyph = "\uE774", Name = "Wireless" },
            new IconItem { Glyph = "\uE774", Name = "USB" },
        };

        public string? SelectedIconGlyph { get; private set; }
        public bool ResetToDefault { get; private set; }

        public IconPickerDialog()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Pre-selects an icon in the grid.
        /// </summary>
        public void SelectIcon(string? glyph)
        {
            if (string.IsNullOrEmpty(glyph)) return;
            
            foreach (var icon in Icons)
            {
                if (icon.Glyph == glyph)
                {
                    IconGridView.SelectedItem = icon;
                    SelectedIconGlyph = glyph; // Also set the selected glyph
                    break;
                }
            }
        }

        private void IconGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IconGridView.SelectedItem is IconItem item)
            {
                SelectedIconGlyph = item.Glyph;
                System.Diagnostics.Debug.WriteLine($"[IconPicker] Selection changed to: {item.Name} ({item.Glyph})");
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ResetToDefault = true;
            SelectedIconGlyph = null;
        }
    }
}
