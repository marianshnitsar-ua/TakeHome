using Domain;
using Xunit;

namespace Domain.UnitTests;

public class MeasurementValidatorTests
{
    private readonly MeasurementValidator _validator = new();

    [Fact]
    public void IsValid_WithValidData_ReturnsTrue()
    {
        // Arrange
        var m = new Measurement(Guid.NewGuid(), DateTimeOffset.UtcNow, "dev1", "p1", "HeartRate", 70, "bpm");

        // Act
        var result = _validator.IsValid(m);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_WithEmptyGuid_ReturnsFalse()
    {
        // Arrange
        var m = new Measurement(Guid.Empty, DateTimeOffset.UtcNow, "dev1", "p1", "HeartRate", 70, "bpm");

        // Act
        var result = _validator.IsValid(m);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_WithDefaultTimestamp_ReturnsFalse()
    {
        // Arrange
        var m = new Measurement(Guid.NewGuid(), default, "dev1", "p1", "HeartRate", 70, "bpm");

        // Act
        var result = _validator.IsValid(m);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("", "HeartRate")] // Empty DeviceId
    [InlineData("dev1", "")]      // Empty Type
    [InlineData(" ", "HeartRate")] // Whitespace DeviceId
    [InlineData("dev1", " ")]      // Whitespace Type
    public void IsValid_WithInvalidStrings_ReturnsFalse(string deviceId, string type)
    {
        // Arrange
        var m = new Measurement(Guid.NewGuid(), DateTimeOffset.UtcNow, deviceId, "p1", type, 70, "bpm");

        // Act
        var result = _validator.IsValid(m);

        // Assert
        Assert.False(result);
    }
}
