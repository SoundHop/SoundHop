using System;

namespace AudioSwitcher.Core.Models
{
    [Flags]
    public enum KeyModifiers : uint
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        Windows = 8
    }

    public class Hotkey
    {
        public KeyModifiers Modifiers { get; set; }
        public int Key { get; set; } // Virtual Key Code

        public override string ToString()
        {
            var parts = new System.Collections.Generic.List<string>();
            if (Modifiers.HasFlag(KeyModifiers.Windows)) parts.Add("Win");
            if (Modifiers.HasFlag(KeyModifiers.Control)) parts.Add("Ctrl");
            if (Modifiers.HasFlag(KeyModifiers.Shift)) parts.Add("Shift");
            if (Modifiers.HasFlag(KeyModifiers.Alt)) parts.Add("Alt");
            
            // Basic Key string representation
            parts.Add(((Windows.System.VirtualKey)Key).ToString()); 

            return string.Join(" + ", parts);
        }
    }
}
