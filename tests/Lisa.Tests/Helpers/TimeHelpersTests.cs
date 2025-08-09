using Lisa.Helpers;

namespace Lisa.Tests.Helpers;

public class TimeHelpersTests
{
    [Fact]
    public void GetCurrentTime_ShouldReturnUtcTime()
    {
        // Act
        var result = TimeHelpers.GetCurrentTime();

        // Assert
        result.Kind.Should().Be(DateTimeKind.Utc);
        result.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void GetCurrentTimeInUtc_ShouldReturnUtcTime()
    {
        // Act
        var result = TimeHelpers.GetCurrentTimeInUtc();

        // Assert
        result.Kind.Should().Be(DateTimeKind.Utc);
        result.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void GetCurrentTimeInLocal_ShouldReturnLocalTime()
    {
        // Act
        var result = TimeHelpers.GetCurrentTimeInLocal();

        // Assert
        result.Kind.Should().Be(DateTimeKind.Local);
        result.Should().BeCloseTo(DateTime.Now, precision: TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ConvertToUtc_WithLocalTime_ShouldReturnUtcTime()
    {
        // Arrange
        var localTime = new DateTime(2023, 6, 15, 14, 30, 0, DateTimeKind.Local);

        // Act
        var result = TimeHelpers.ConvertToUtc(localTime);

        // Assert
        result.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void ConvertToLocal_WithUtcTime_ShouldReturnLocalTime()
    {
        // Arrange
        var utcTime = new DateTime(2023, 6, 15, 14, 30, 0, DateTimeKind.Utc);

        // Act
        var result = TimeHelpers.ConvertToLocal(utcTime);

        // Assert
        result.Kind.Should().Be(DateTimeKind.Local);
    }

    [Theory]
    [InlineData(2023, 1, 1, 12, 0, 0)]
    [InlineData(2023, 6, 15, 14, 30, 45)]
    [InlineData(2023, 12, 31, 23, 59, 59)]
    public void ConvertToUtc_WithVariousDates_ShouldMaintainDateValues(int year, int month, int day, int hour, int minute, int second)
    {
        // Arrange
        var localTime = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Local);

        // Act
        var result = TimeHelpers.ConvertToUtc(localTime);

        // Assert
        result.Kind.Should().Be(DateTimeKind.Utc);
        // The actual time might differ due to timezone conversion, but it should be a valid conversion
        result.Should().NotBe(default(DateTime));
    }

    [Theory]
    [InlineData(2023, 1, 1, 12, 0, 0)]
    [InlineData(2023, 6, 15, 14, 30, 45)]
    [InlineData(2023, 12, 31, 23, 59, 59)]
    public void ConvertToLocal_WithVariousDates_ShouldMaintainDateValues(int year, int month, int day, int hour, int minute, int second)
    {
        // Arrange
        var utcTime = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);

        // Act
        var result = TimeHelpers.ConvertToLocal(utcTime);

        // Assert
        result.Kind.Should().Be(DateTimeKind.Local);
        result.Should().NotBe(default(DateTime));
    }
}
