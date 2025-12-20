using AudioSwitcher.Core.Models;
using Xunit;

namespace AudioSwitcher.Tests.Models;

/// <summary>
/// Tests for DeviceNameInfo and HotkeyDisplayInfo model classes.
/// </summary>
public class DeviceInfoTests
{
    #region DeviceNameInfo Tests

    [Fact]
    public void DeviceNameInfo_DefaultDisplayName_IsEmpty()
    {
        var info = new DeviceNameInfo();
        Assert.Equal(string.Empty, info.DisplayName);
    }

    [Fact]
    public void DeviceNameInfo_DefaultSubName_IsEmpty()
    {
        var info = new DeviceNameInfo();
        Assert.Equal(string.Empty, info.SubName);
    }

    [Fact]
    public void DeviceNameInfo_CanSetDisplayName()
    {
        var info = new DeviceNameInfo { DisplayName = "Speakers" };
        Assert.Equal("Speakers", info.DisplayName);
    }

    [Fact]
    public void DeviceNameInfo_CanSetSubName()
    {
        var info = new DeviceNameInfo { SubName = "Realtek Audio" };
        Assert.Equal("Realtek Audio", info.SubName);
    }

    #endregion

    #region HotkeyDisplayInfo Tests

    [Fact]
    public void HotkeyDisplayInfo_DefaultDeviceId_IsEmpty()
    {
        var info = new HotkeyDisplayInfo();
        Assert.Equal(string.Empty, info.DeviceId);
    }

    [Fact]
    public void HotkeyDisplayInfo_DefaultHotkey_IsNull()
    {
        var info = new HotkeyDisplayInfo();
        Assert.Null(info.Hotkey);
    }

    [Fact]
    public void HotkeyDisplayInfo_DefaultIsConnected_IsFalse()
    {
        var info = new HotkeyDisplayInfo();
        Assert.False(info.IsConnected);
    }

    [Fact]
    public void HotkeyDisplayInfo_DefaultDevice_IsNull()
    {
        var info = new HotkeyDisplayInfo();
        Assert.Null(info.Device);
    }

    [Fact]
    public void HotkeyDisplayInfo_DefaultDisplayName_IsEmpty()
    {
        var info = new HotkeyDisplayInfo();
        Assert.Equal(string.Empty, info.DisplayName);
    }

    [Fact]
    public void HotkeyDisplayInfo_DefaultSubName_IsEmpty()
    {
        var info = new HotkeyDisplayInfo();
        Assert.Equal(string.Empty, info.SubName);
    }

    [Fact]
    public void HotkeyDisplayInfo_HasSubName_WithSubName_ReturnsTrue()
    {
        var info = new HotkeyDisplayInfo { SubName = "USB Audio Device" };
        Assert.True(info.HasSubName);
    }

    [Fact]
    public void HotkeyDisplayInfo_HasSubName_WithEmptySubName_ReturnsFalse()
    {
        var info = new HotkeyDisplayInfo { SubName = "" };
        Assert.False(info.HasSubName);
    }

    [Fact]
    public void HotkeyDisplayInfo_HasSubName_WithNullSubName_ReturnsFalse()
    {
        var info = new HotkeyDisplayInfo { SubName = null! };
        Assert.False(info.HasSubName);
    }

    [Fact]
    public void HotkeyDisplayInfo_CanSetHotkey()
    {
        var hotkey = new Hotkey { Modifiers = KeyModifiers.Control, Key = 65 };
        var info = new HotkeyDisplayInfo { Hotkey = hotkey };
        
        Assert.NotNull(info.Hotkey);
        Assert.Equal(KeyModifiers.Control, info.Hotkey.Modifiers);
        Assert.Equal(65, info.Hotkey.Key);
    }

    [Fact]
    public void HotkeyDisplayInfo_CanSetDevice()
    {
        var device = new AudioDevice { Id = "device-1", Name = "Test Device" };
        var info = new HotkeyDisplayInfo { Device = device };
        
        Assert.NotNull(info.Device);
        Assert.Equal("device-1", info.Device.Id);
    }

    [Fact]
    public void HotkeyDisplayInfo_CanSetIsConnected()
    {
        var info = new HotkeyDisplayInfo { IsConnected = true };
        Assert.True(info.IsConnected);
    }

    #endregion
}
