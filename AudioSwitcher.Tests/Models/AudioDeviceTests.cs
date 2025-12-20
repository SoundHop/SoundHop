using AudioSwitcher.Core.Models;
using Xunit;

namespace AudioSwitcher.Tests.Models;

/// <summary>
/// Tests for the AudioDevice model class.
/// </summary>
public class AudioDeviceTests
{
    #region DisplayIcon Tests

    [Fact]
    public void DisplayIcon_WithCustomIconGlyph_ReturnsCustomIcon()
    {
        var device = new AudioDevice
        {
            Name = "Test Speakers",
            IconPath = "@%SystemRoot%\\System32\\mmres.dll,-3010",
            CustomIconGlyph = "\uE8D6" // Custom icon
        };

        Assert.Equal("\uE8D6", device.DisplayIcon);
    }

    [Theory]
    [InlineData("-3010", "\uE7F5")] // Speakers
    [InlineData("-3011", "\uE7F6")] // Headphones
    [InlineData("-3012", "\uE7F6")] // Headset
    [InlineData("-3013", "\uE7F3")] // Digital/SPDIF
    [InlineData("-3014", "\uE7F5")] // Line Out
    [InlineData("-3015", "\uE7F4")] // Monitor
    [InlineData("-3030", "\uE720")] // Microphone
    [InlineData("-3031", "\uE720")] // Microphone
    [InlineData("-9999", "\uE7F4")] // Unknown -> Default Monitor
    public void DisplayIcon_WithMmresIconPath_ReturnsCorrectIcon(string iconId, string expectedIcon)
    {
        var device = new AudioDevice
        {
            Name = "Test Device",
            IconPath = $"@%SystemRoot%\\System32\\mmres.dll,{iconId}"
        };

        Assert.Equal(expectedIcon, device.DisplayIcon);
    }

    [Fact]
    public void DisplayIcon_WithNonMmresPath_ReturnsMusicNoteIcon()
    {
        var device = new AudioDevice
        {
            Name = "Test Device",
            IconPath = "@%SystemRoot%\\System32\\customaudio.dll,-100"
        };

        Assert.Equal("\uE767", device.DisplayIcon); // Music note fallback
    }

    [Fact]
    public void DisplayIcon_WithEmptyIconPath_UsesNameHeuristic_Headphones()
    {
        var device = new AudioDevice { Name = "My Headphones (USB Device)", IconPath = "" };
        Assert.Equal("\uE7F6", device.DisplayIcon);
    }

    [Fact]
    public void DisplayIcon_WithEmptyIconPath_UsesNameHeuristic_Headset()
    {
        var device = new AudioDevice { Name = "Gaming Headset", IconPath = "" };
        Assert.Equal("\uE7F6", device.DisplayIcon);
    }

    [Fact]
    public void DisplayIcon_WithEmptyIconPath_UsesNameHeuristic_Speakers()
    {
        var device = new AudioDevice { Name = "Desktop Speakers", IconPath = "" };
        Assert.Equal("\uE7F5", device.DisplayIcon);
    }

    [Fact]
    public void DisplayIcon_WithEmptyIconPath_NoMatch_ReturnsDefaultMonitor()
    {
        var device = new AudioDevice { Name = "Unknown Audio Device", IconPath = "" };
        Assert.Equal("\uE7F4", device.DisplayIcon);
    }

    [Fact]
    public void DisplayIcon_WithMalformedIconPath_ReturnsDefaultMonitor()
    {
        var device = new AudioDevice
        {
            Name = "Test Device",
            IconPath = "@%SystemRoot%\\System32\\mmres.dll" // Missing comma and ID
        };

        Assert.Equal("\uE7F4", device.DisplayIcon);
    }

    [Fact]
    public void DisplayIcon_WithInvalidIconId_ReturnsDefaultMonitor()
    {
        var device = new AudioDevice
        {
            Name = "Test Device",
            IconPath = "@%SystemRoot%\\System32\\mmres.dll,notanumber"
        };

        Assert.Equal("\uE7F4", device.DisplayIcon);
    }

    #endregion

    #region DisplayName/DisplaySubName Tests

    [Fact]
    public void DisplayName_WithParentheses_ExtractsMainName()
    {
        var device = new AudioDevice { Name = "Speakers (Realtek High Definition Audio)" };
        Assert.Equal("Speakers", device.DisplayName);
    }

    [Fact]
    public void DisplaySubName_WithParentheses_ExtractsSubName()
    {
        var device = new AudioDevice { Name = "Speakers (Realtek High Definition Audio)" };
        Assert.Equal("Realtek High Definition Audio", device.DisplaySubName);
    }

    [Fact]
    public void DisplayName_WithoutParentheses_ReturnsFullName()
    {
        var device = new AudioDevice { Name = "Simple Device Name" };
        Assert.Equal("Simple Device Name", device.DisplayName);
    }

    [Fact]
    public void DisplaySubName_WithoutParentheses_ReturnsEmptyString()
    {
        var device = new AudioDevice { Name = "Simple Device Name" };
        Assert.Equal(string.Empty, device.DisplaySubName);
    }

    [Fact]
    public void HasSubName_WithSubName_ReturnsTrue()
    {
        var device = new AudioDevice { Name = "Speakers (USB Audio)" };
        Assert.True(device.HasSubName);
    }

    [Fact]
    public void HasSubName_WithoutSubName_ReturnsFalse()
    {
        var device = new AudioDevice { Name = "Simple Name" };
        Assert.False(device.HasSubName);
    }

    [Fact]
    public void DisplayName_WithNestedParentheses_ExtractsCorrectly()
    {
        var device = new AudioDevice { Name = "MAIN Left/Right (Minifuse 1)" };
        Assert.Equal("MAIN Left/Right", device.DisplayName);
        Assert.Equal("Minifuse 1", device.DisplaySubName);
    }

    #endregion

    #region State Property Tests

    [Theory]
    [InlineData(1u, true)]  // Active
    [InlineData(2u, false)] // Disabled
    [InlineData(8u, false)] // Unplugged
    [InlineData(4u, false)] // Not present
    public void IsActive_ReturnsCorrectValue(uint state, bool expected)
    {
        var device = new AudioDevice { State = state };
        Assert.Equal(expected, device.IsActive);
    }

    [Theory]
    [InlineData(2u, true)]  // Disabled
    [InlineData(1u, false)] // Active
    [InlineData(8u, false)] // Unplugged
    public void IsDisabled_ReturnsCorrectValue(uint state, bool expected)
    {
        var device = new AudioDevice { State = state };
        Assert.Equal(expected, device.IsDisabled);
    }

    [Theory]
    [InlineData(8u, true)]  // Unplugged
    [InlineData(1u, false)] // Active
    [InlineData(2u, false)] // Disabled
    public void IsDisconnected_ReturnsCorrectValue(uint state, bool expected)
    {
        var device = new AudioDevice { State = state };
        Assert.Equal(expected, device.IsDisconnected);
    }

    #endregion

    #region PropertyChanged Tests

    [Fact]
    public void IsDefault_WhenChanged_RaisesPropertyChanged()
    {
        var device = new AudioDevice();
        var propertyName = string.Empty;
        device.PropertyChanged += (s, e) => propertyName = e.PropertyName;

        device.IsDefault = true;

        Assert.Equal(nameof(AudioDevice.IsDefault), propertyName);
    }

    [Fact]
    public void IsFavorite_WhenChanged_RaisesPropertyChanged()
    {
        var device = new AudioDevice();
        var propertyName = string.Empty;
        device.PropertyChanged += (s, e) => propertyName = e.PropertyName;

        device.IsFavorite = true;

        Assert.Equal(nameof(AudioDevice.IsFavorite), propertyName);
    }

    [Fact]
    public void State_WhenChanged_RaisesMultiplePropertyChanged()
    {
        var device = new AudioDevice();
        var raisedProperties = new List<string>();
        device.PropertyChanged += (s, e) => raisedProperties.Add(e.PropertyName!);

        device.State = 2;

        Assert.Contains(nameof(AudioDevice.State), raisedProperties);
        Assert.Contains(nameof(AudioDevice.IsDisabled), raisedProperties);
        Assert.Contains(nameof(AudioDevice.IsDisconnected), raisedProperties);
        Assert.Contains(nameof(AudioDevice.IsActive), raisedProperties);
    }

    [Fact]
    public void CustomIconGlyph_WhenChanged_RaisesDisplayIconPropertyChanged()
    {
        var device = new AudioDevice();
        var raisedProperties = new List<string>();
        device.PropertyChanged += (s, e) => raisedProperties.Add(e.PropertyName!);

        device.CustomIconGlyph = "\uE8D6";

        Assert.Contains(nameof(AudioDevice.CustomIconGlyph), raisedProperties);
        Assert.Contains(nameof(AudioDevice.DisplayIcon), raisedProperties);
    }

    [Fact]
    public void IsDefault_WhenSetToSameValue_DoesNotRaisePropertyChanged()
    {
        var device = new AudioDevice { IsDefault = true };
        var eventRaised = false;
        device.PropertyChanged += (s, e) => eventRaised = true;

        device.IsDefault = true; // Same value

        Assert.False(eventRaised);
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ReturnsName()
    {
        var device = new AudioDevice { Name = "My Audio Device" };
        Assert.Equal("My Audio Device", device.ToString());
    }

    #endregion
}
