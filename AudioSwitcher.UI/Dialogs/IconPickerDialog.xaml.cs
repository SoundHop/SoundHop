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
            new IconItem { Glyph = "\uE95B", Name = "Headset" },
            new IconItem { Glyph = "\uF4C0", Name = "Earbuds" },
            new IconItem { Glyph = "\uE767", Name = "Volume" },
            new IconItem { Glyph = "\uE95F", Name = "Wire" },

            // TV & Displays
            new IconItem { Glyph = "\uE7F4", Name = "Monitor" },
            new IconItem { Glyph = "\uE7F3", Name = "Display Sound" },
            new IconItem { Glyph = "\uE7F8", Name = "Laptop" },
            new IconItem { Glyph = "\uE95D", Name = "Projector" },
            
            // Audio Input
            new IconItem { Glyph = "\uE720", Name = "Microphone" },
            new IconItem { Glyph = "\uE960", Name = "Webcam" },
            
            // Audio/Music
            new IconItem { Glyph = "\uEC4F", Name = "Music" },
            new IconItem { Glyph = "\uE8D6", Name = "Audio" },
            new IconItem { Glyph = "\uE93C", Name = "Album" },
            new IconItem { Glyph = "\uEA69", Name = "Media" },
            
            // Gaming & VR
            new IconItem { Glyph = "\uE990", Name = "Xbox" },
            new IconItem { Glyph = "\uE967", Name = "Game Console" },
            new IconItem { Glyph = "\uE7FC", Name = "Gaming" },
            new IconItem { Glyph = "\uF119", Name = "VR Headset" },
            
            // Misc
            new IconItem { Glyph = "\uE702", Name = "Bluetooth" },
            new IconItem { Glyph = "\uE701", Name = "Wireless" },
            new IconItem { Glyph = "\uE93E", Name = "Streaming" },
            new IconItem { Glyph = "\uEC15", Name = "Cast" },
            new IconItem { Glyph = "\uE95A", Name = "Communications" },
            new IconItem { Glyph = "\uE88E", Name = "USB" },
            new IconItem { Glyph = "\uE954", Name = "DVR" },
            new IconItem { Glyph = "\uE955", Name = "MultimediaPMP" },
            new IconItem { Glyph = "\uED47", Name = "MultimediaDMP" },
            new IconItem { Glyph = "\uE96A", Name = "Casette Tape" },
            new IconItem { Glyph = "\uF61F", Name = "Noise Cancellation" },
            new IconItem { Glyph = "\uE9E9", Name = "Equalizer" },
            new IconItem { Glyph = "\uE772", Name = "Devices" },

            // Mobile & Phones
            new IconItem { Glyph = "\uE8EA", Name = "Phone" },
            new IconItem { Glyph = "\uE717", Name = "Cell Phone" },
            new IconItem { Glyph = "\uE70A", Name = "Tablet" },
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
