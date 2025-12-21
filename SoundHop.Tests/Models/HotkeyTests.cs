using SoundHop.Core.Models;
using Xunit;

namespace SoundHop.Tests.Models;

/// <summary>
/// Tests for the Hotkey and KeyModifiers classes.
/// </summary>
public class HotkeyTests
{
    #region KeyModifiers Flags Tests

    [Fact]
    public void KeyModifiers_None_HasValueZero()
    {
        Assert.Equal(0u, (uint)KeyModifiers.None);
    }

    [Fact]
    public void KeyModifiers_CanCombineFlags()
    {
        var modifiers = KeyModifiers.Control | KeyModifiers.Alt;
        
        Assert.True(modifiers.HasFlag(KeyModifiers.Control));
        Assert.True(modifiers.HasFlag(KeyModifiers.Alt));
        Assert.False(modifiers.HasFlag(KeyModifiers.Shift));
        Assert.False(modifiers.HasFlag(KeyModifiers.Windows));
    }

    [Fact]
    public void KeyModifiers_AllFlags_CanBeCombined()
    {
        var all = KeyModifiers.Alt | KeyModifiers.Control | KeyModifiers.Shift | KeyModifiers.Windows;
        
        Assert.True(all.HasFlag(KeyModifiers.Alt));
        Assert.True(all.HasFlag(KeyModifiers.Control));
        Assert.True(all.HasFlag(KeyModifiers.Shift));
        Assert.True(all.HasFlag(KeyModifiers.Windows));
    }

    #endregion

    #region Hotkey Property Tests

    [Fact]
    public void Hotkey_DefaultModifiers_IsNone()
    {
        var hotkey = new Hotkey();
        Assert.Equal(KeyModifiers.None, hotkey.Modifiers);
    }

    [Fact]
    public void Hotkey_DefaultKey_IsZero()
    {
        var hotkey = new Hotkey();
        Assert.Equal(0, hotkey.Key);
    }

    [Fact]
    public void Hotkey_CanSetModifiers()
    {
        var hotkey = new Hotkey { Modifiers = KeyModifiers.Control | KeyModifiers.Shift };
        
        Assert.True(hotkey.Modifiers.HasFlag(KeyModifiers.Control));
        Assert.True(hotkey.Modifiers.HasFlag(KeyModifiers.Shift));
    }

    [Fact]
    public void Hotkey_CanSetKey()
    {
        var hotkey = new Hotkey { Key = 65 }; // 'A' key
        Assert.Equal(65, hotkey.Key);
    }

    #endregion
}
