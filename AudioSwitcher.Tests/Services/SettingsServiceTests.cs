using AudioSwitcher.Core.Models;
using AudioSwitcher.Core.Services;
using System.Reflection;
using Xunit;

namespace AudioSwitcher.Tests.Services;

/// <summary>
/// Tests for the SettingsService class.
/// Uses a testable wrapper to avoid singleton issues.
/// </summary>
public class SettingsServiceTests
{
    /// <summary>
    /// Creates a fresh AppSettings instance for testing.
    /// </summary>
    private static SettingsService.AppSettings CreateSettings() => new();

    #region AppSettings Default Values Tests

    [Fact]
    public void AppSettings_DefaultFavoriteDeviceIds_IsEmpty()
    {
        var settings = CreateSettings();
        Assert.Empty(settings.FavoriteDeviceIds);
    }

    [Fact]
    public void AppSettings_DefaultHotkeys_IsEmpty()
    {
        var settings = CreateSettings();
        Assert.Empty(settings.Hotkeys);
    }

    [Fact]
    public void AppSettings_DefaultShowTrayIcon_IsTrue()
    {
        var settings = CreateSettings();
        Assert.True(settings.ShowTrayIcon);
    }

    [Fact]
    public void AppSettings_DefaultMinimizeToTray_IsFalse()
    {
        var settings = CreateSettings();
        Assert.False(settings.MinimizeToTray);
    }

    [Fact]
    public void AppSettings_DefaultCloseToTray_IsTrue()
    {
        var settings = CreateSettings();
        Assert.True(settings.CloseToTray);
    }

    [Fact]
    public void AppSettings_DefaultStartMinimized_IsTrue()
    {
        var settings = CreateSettings();
        Assert.True(settings.StartMinimized);
    }

    [Fact]
    public void AppSettings_DefaultQuickSwitchMode_IsFalse()
    {
        var settings = CreateSettings();
        Assert.False(settings.QuickSwitchMode);
    }

    [Fact]
    public void AppSettings_DefaultSyncCommunicationDevice_IsTrue()
    {
        var settings = CreateSettings();
        Assert.True(settings.SyncCommunicationDevice);
    }

    [Fact]
    public void AppSettings_DefaultShowDisabledDevices_IsFalse()
    {
        var settings = CreateSettings();
        Assert.False(settings.ShowDisabledDevices);
    }

    [Fact]
    public void AppSettings_DefaultShowDisconnectedDevices_IsFalse()
    {
        var settings = CreateSettings();
        Assert.False(settings.ShowDisconnectedDevices);
    }

    [Fact]
    public void AppSettings_DefaultDeviceSortMode_IsFriendlyName()
    {
        var settings = CreateSettings();
        Assert.Equal("FriendlyName", settings.DeviceSortMode);
    }

    [Fact]
    public void AppSettings_DefaultCustomDeviceIcons_IsEmpty()
    {
        var settings = CreateSettings();
        Assert.Empty(settings.CustomDeviceIcons);
    }

    [Fact]
    public void AppSettings_DefaultDeviceNameMapping_IsEmpty()
    {
        var settings = CreateSettings();
        Assert.Empty(settings.DeviceNameMapping);
    }

    #endregion

    #region FindOldIdByName Tests (via reflection to access singleton's Settings)

    [Fact]
    public void FindOldIdByName_WithMatchingDevice_ReturnsOldId()
    {
        // Arrange
        var service = SettingsService.Instance;
        var settings = service.Settings;
        
        // Clear and setup test data
        settings.DeviceNameMapping.Clear();
        settings.DeviceNameMapping["old-device-id"] = new DeviceNameInfo 
        { 
            DisplayName = "Speakers", 
            SubName = "Realtek Audio" 
        };
        settings.DeviceNameMapping["current-device-id"] = new DeviceNameInfo 
        { 
            DisplayName = "Speakers", 
            SubName = "Realtek Audio" 
        };

        // Act
        var result = service.FindOldIdByName("Speakers", "Realtek Audio", "current-device-id");

        // Assert
        Assert.Equal("old-device-id", result);
        
        // Cleanup
        settings.DeviceNameMapping.Clear();
    }

    [Fact]
    public void FindOldIdByName_WithNoMatch_ReturnsNull()
    {
        var service = SettingsService.Instance;
        var settings = service.Settings;
        
        settings.DeviceNameMapping.Clear();
        settings.DeviceNameMapping["other-device"] = new DeviceNameInfo 
        { 
            DisplayName = "Headphones", 
            SubName = "USB Audio" 
        };

        var result = service.FindOldIdByName("Speakers", "Realtek Audio", "current-id");

        Assert.Null(result);
        
        settings.DeviceNameMapping.Clear();
    }

    [Fact]
    public void FindOldIdByName_SkipsCurrentId()
    {
        var service = SettingsService.Instance;
        var settings = service.Settings;
        
        settings.DeviceNameMapping.Clear();
        settings.DeviceNameMapping["device-id"] = new DeviceNameInfo 
        { 
            DisplayName = "Speakers", 
            SubName = "Realtek Audio" 
        };

        // Search with the same ID as current - should not find itself
        var result = service.FindOldIdByName("Speakers", "Realtek Audio", "device-id");

        Assert.Null(result);
        
        settings.DeviceNameMapping.Clear();
    }

    [Fact]
    public void FindOldIdByName_RequiresBothDisplayNameAndSubName()
    {
        var service = SettingsService.Instance;
        var settings = service.Settings;
        
        settings.DeviceNameMapping.Clear();
        settings.DeviceNameMapping["old-id"] = new DeviceNameInfo 
        { 
            DisplayName = "Speakers", 
            SubName = "Realtek Audio" 
        };

        // Same SubName but different DisplayName - should not match
        var result = service.FindOldIdByName("Headphones", "Realtek Audio", "current-id");

        Assert.Null(result);
        
        settings.DeviceNameMapping.Clear();
    }

    #endregion

    #region GetDeviceNameInfo Tests

    [Fact]
    public void GetDeviceNameInfo_WithKnownId_ReturnsInfo()
    {
        var service = SettingsService.Instance;
        var settings = service.Settings;
        
        settings.DeviceNameMapping.Clear();
        settings.DeviceNameMapping["known-id"] = new DeviceNameInfo 
        { 
            DisplayName = "Test Device", 
            SubName = "Test Adapter" 
        };

        var result = service.GetDeviceNameInfo("known-id");

        Assert.NotNull(result);
        Assert.Equal("Test Device", result.DisplayName);
        Assert.Equal("Test Adapter", result.SubName);
        
        settings.DeviceNameMapping.Clear();
    }

    [Fact]
    public void GetDeviceNameInfo_WithUnknownId_ReturnsNull()
    {
        var service = SettingsService.Instance;
        var settings = service.Settings;
        
        settings.DeviceNameMapping.Clear();

        var result = service.GetDeviceNameInfo("unknown-id");

        Assert.Null(result);
    }

    #endregion

    #region MigrateDeviceSettings Tests

    [Fact]
    public void MigrateDeviceSettings_MigratesFavorites()
    {
        var service = SettingsService.Instance;
        var settings = service.Settings;
        
        // Setup
        settings.FavoriteDeviceIds.Clear();
        settings.DeviceNameMapping.Clear();
        settings.Hotkeys.Clear();
        settings.CustomDeviceIcons.Clear();
        
        settings.FavoriteDeviceIds.Add("old-id");

        // Act
        service.MigrateDeviceSettings("old-id", "new-id", "Display", "Sub");

        // Assert
        Assert.DoesNotContain("old-id", settings.FavoriteDeviceIds);
        Assert.Contains("new-id", settings.FavoriteDeviceIds);
        
        // Cleanup
        settings.FavoriteDeviceIds.Clear();
        settings.DeviceNameMapping.Clear();
    }

    [Fact]
    public void MigrateDeviceSettings_MigratesHotkeys()
    {
        var service = SettingsService.Instance;
        var settings = service.Settings;
        
        settings.FavoriteDeviceIds.Clear();
        settings.DeviceNameMapping.Clear();
        settings.Hotkeys.Clear();
        settings.CustomDeviceIcons.Clear();
        
        var hotkey = new Hotkey { Modifiers = KeyModifiers.Control, Key = 65 };
        settings.Hotkeys["old-id"] = hotkey;

        service.MigrateDeviceSettings("old-id", "new-id", "Display", "Sub");

        Assert.False(settings.Hotkeys.ContainsKey("old-id"));
        Assert.True(settings.Hotkeys.ContainsKey("new-id"));
        Assert.Equal(KeyModifiers.Control, settings.Hotkeys["new-id"].Modifiers);
        
        settings.Hotkeys.Clear();
        settings.DeviceNameMapping.Clear();
    }

    [Fact]
    public void MigrateDeviceSettings_MigratesCustomIcons()
    {
        var service = SettingsService.Instance;
        var settings = service.Settings;
        
        settings.FavoriteDeviceIds.Clear();
        settings.DeviceNameMapping.Clear();
        settings.Hotkeys.Clear();
        settings.CustomDeviceIcons.Clear();
        
        settings.CustomDeviceIcons["old-id"] = "\uE8D6";

        service.MigrateDeviceSettings("old-id", "new-id", "Display", "Sub");

        Assert.False(settings.CustomDeviceIcons.ContainsKey("old-id"));
        Assert.True(settings.CustomDeviceIcons.ContainsKey("new-id"));
        Assert.Equal("\uE8D6", settings.CustomDeviceIcons["new-id"]);
        
        settings.CustomDeviceIcons.Clear();
        settings.DeviceNameMapping.Clear();
    }

    [Fact]
    public void MigrateDeviceSettings_UpdatesNameMapping()
    {
        var service = SettingsService.Instance;
        var settings = service.Settings;
        
        settings.FavoriteDeviceIds.Clear();
        settings.DeviceNameMapping.Clear();
        settings.Hotkeys.Clear();
        settings.CustomDeviceIcons.Clear();
        
        settings.DeviceNameMapping["old-id"] = new DeviceNameInfo { DisplayName = "Old", SubName = "Old Sub" };

        service.MigrateDeviceSettings("old-id", "new-id", "New Display", "New Sub");

        Assert.False(settings.DeviceNameMapping.ContainsKey("old-id"));
        Assert.True(settings.DeviceNameMapping.ContainsKey("new-id"));
        Assert.Equal("New Display", settings.DeviceNameMapping["new-id"].DisplayName);
        Assert.Equal("New Sub", settings.DeviceNameMapping["new-id"].SubName);
        
        settings.DeviceNameMapping.Clear();
    }

    #endregion

    #region UpdateDeviceNameMapping Tests

    [Fact]
    public void UpdateDeviceNameMapping_AddsNewMapping()
    {
        var service = SettingsService.Instance;
        var settings = service.Settings;
        
        settings.DeviceNameMapping.Clear();

        service.UpdateDeviceNameMapping("new-device", "Display Name", "Sub Name");

        Assert.True(settings.DeviceNameMapping.ContainsKey("new-device"));
        Assert.Equal("Display Name", settings.DeviceNameMapping["new-device"].DisplayName);
        Assert.Equal("Sub Name", settings.DeviceNameMapping["new-device"].SubName);
        
        settings.DeviceNameMapping.Clear();
    }

    [Fact]
    public void UpdateDeviceNameMapping_UpdatesExistingMapping()
    {
        var service = SettingsService.Instance;
        var settings = service.Settings;
        
        settings.DeviceNameMapping.Clear();
        settings.DeviceNameMapping["device-id"] = new DeviceNameInfo { DisplayName = "Old", SubName = "Old Sub" };

        service.UpdateDeviceNameMapping("device-id", "New Display", "New Sub");

        Assert.Equal("New Display", settings.DeviceNameMapping["device-id"].DisplayName);
        Assert.Equal("New Sub", settings.DeviceNameMapping["device-id"].SubName);
        
        settings.DeviceNameMapping.Clear();
    }

    #endregion
}
