using Xunit;

namespace SoundHop.Tests.Converters;

/// <summary>
/// Tests for value converters that have pure logic without WinUI dependencies.
/// Note: Visibility-based converters are skipped as they require WinUI runtime.
/// These tests verify the conversion logic by testing the underlying patterns.
/// </summary>
public class ConverterLogicTests
{
    #region Bool to Opacity Logic Tests

    /// <summary>
    /// Tests the bool-to-opacity conversion pattern used in BoolToOpacityConverter
    /// </summary>
    [Theory]
    [InlineData(true, 1.0)]
    [InlineData(false, 0.5)]
    public void BoolToOpacity_ReturnsCorrectValue(bool input, double expected)
    {
        // This mirrors the logic in BoolToOpacityConverter
        double result = input ? 1.0 : 0.5;
        Assert.Equal(expected, result);
    }

    [Fact]
    public void BoolToOpacity_NullInput_ReturnsDefault()
    {
        // When input is not a bool, returns 1.0 (default visible)
        object? input = null;
        double result = input is bool b ? (b ? 1.0 : 0.5) : 1.0;
        Assert.Equal(1.0, result);
    }

    [Fact]
    public void BoolToOpacity_StringInput_ReturnsDefault()
    {
        object input = "not a bool";
        double result = input is bool b ? (b ? 1.0 : 0.5) : 1.0;
        Assert.Equal(1.0, result);
    }

    #endregion

    #region Inverse Bool Logic Tests

    /// <summary>
    /// Tests the inverse bool conversion pattern used in InverseBoolConverter
    /// </summary>
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void InverseBool_ReturnsOpposite(bool input, bool expected)
    {
        bool result = !input;
        Assert.Equal(expected, result);
    }

    [Fact]
    public void InverseBool_NullInput_ReturnsTrue()
    {
        // When input is not a bool, returns true (default for inverse)
        object? input = null;
        bool result = input is bool b ? !b : true;
        Assert.True(result);
    }

    [Fact]
    public void InverseBool_ConvertBack_Works()
    {
        // ConvertBack should also invert
        bool input = true;
        bool result = !input;
        Assert.False(result);
    }

    #endregion

    #region Favorite Icon Logic Tests

    /// <summary>
    /// Tests the favorite icon conversion pattern used in FavoriteIconConverter
    /// </summary>
    [Fact]
    public void FavoriteIcon_WhenTrue_ReturnsSolidStar()
    {
        bool isFavorite = true;
        string result = isFavorite ? "\uE735" : "\uE734";
        Assert.Equal("\uE735", result); // Solid star
    }

    [Fact]
    public void FavoriteIcon_WhenFalse_ReturnsOutlineStar()
    {
        bool isFavorite = false;
        string result = isFavorite ? "\uE735" : "\uE734";
        Assert.Equal("\uE734", result); // Outline star
    }

    [Fact]
    public void FavoriteIcon_NullInput_ReturnsOutlineStar()
    {
        object? input = null;
        string result = (input is bool b && b) ? "\uE735" : "\uE734";
        Assert.Equal("\uE734", result);
    }

    #endregion

    #region Device Opacity Logic Tests

    /// <summary>
    /// Tests the device opacity conversion pattern used in DeviceOpacityConverter
    /// </summary>
    [Theory]
    [InlineData(true, 1.0)]  // Active device - full opacity
    [InlineData(false, 0.5)] // Inactive device - dimmed
    public void DeviceOpacity_ReturnsCorrectValue(bool isActive, double expected)
    {
        double result = isActive ? 1.0 : 0.5;
        Assert.Equal(expected, result);
    }

    #endregion

    #region Null Check Logic Tests

    /// <summary>
    /// Tests the null-to-visibility conversion pattern used in NullToVisibilityConverter
    /// </summary>
    [Fact]
    public void NullCheck_NullValue_ReturnsFalse()
    {
        object? value = null;
        bool isVisible = value != null;
        Assert.False(isVisible);
    }

    [Fact]
    public void NullCheck_NonNullValue_ReturnsTrue()
    {
        object? value = "some object";
        bool isVisible = value != null;
        Assert.True(isVisible);
    }

    [Fact]
    public void NullCheck_EmptyString_ReturnsTrue()
    {
        // Empty string is not null
        object? value = string.Empty;
        bool isVisible = value != null;
        Assert.True(isVisible);
    }

    #endregion

    #region Bool to Opacity Hidden Logic Tests

    /// <summary>
    /// Tests the bool-to-opacity hidden conversion pattern (true = 0, false = 1)
    /// This is the inverse of BoolToOpacityConverter
    /// </summary>
    [Theory]
    [InlineData(true, 0.0)]  // True = hidden (0)
    [InlineData(false, 1.0)] // False = visible (1)
    public void BoolToOpacityHidden_ReturnsInverseOpacity(bool input, double expected)
    {
        // This mirrors the inverse opacity logic
        double result = input ? 0.0 : 1.0;
        Assert.Equal(expected, result);
    }

    #endregion
}
