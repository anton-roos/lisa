using Lisa.Enums;

namespace Lisa.Tests.Enums;

public class EnumTests
{
    [Theory]
    [InlineData(Gender.None, 0)]
    [InlineData(Gender.Male, 1)]
    [InlineData(Gender.Female, 2)]
    public void Gender_ShouldHaveCorrectValues(Gender gender, int expectedValue)
    {
        // Act & Assert
        ((int)gender).Should().Be(expectedValue);
    }

    [Fact]
    public void Gender_ShouldHaveExpectedMembers()
    {
        // Act
        var genderValues = Enum.GetValues<Gender>();

        // Assert
        genderValues.Should().Contain(Gender.None);
        genderValues.Should().Contain(Gender.Male);
        genderValues.Should().Contain(Gender.Female);
        genderValues.Should().HaveCount(3);
    }

    [Theory]
    [InlineData("None", Gender.None)]
    [InlineData("Male", Gender.Male)]
    [InlineData("Female", Gender.Female)]
    public void Gender_ShouldParseFromString(string genderString, Gender expectedGender)
    {
        // Act
        var parsed = Enum.Parse<Gender>(genderString);

        // Assert
        parsed.Should().Be(expectedGender);
    }

    [Theory]
    [InlineData(AttendanceType.CheckIn, 0)]
    [InlineData(AttendanceType.CheckOut, 1)]
    [InlineData(AttendanceType.Register, 2)]
    [InlineData(AttendanceType.Period, 3)]
    [InlineData(AttendanceType.Adi, 4)]
    [InlineData(AttendanceType.CheckInFromRegister, 5)]
    [InlineData(AttendanceType.CheckInFromPeriod, 6)]
    public void AttendanceType_ShouldHaveCorrectValues(AttendanceType attendanceType, int expectedValue)
    {
        // Act & Assert
        ((int)attendanceType).Should().Be(expectedValue);
    }

    [Fact]
    public void AttendanceType_ShouldHaveExpectedMembers()
    {
        // Act
        var attendanceTypeValues = Enum.GetValues<AttendanceType>();

        // Assert
        attendanceTypeValues.Should().Contain(AttendanceType.CheckIn);
        attendanceTypeValues.Should().Contain(AttendanceType.CheckOut);
        attendanceTypeValues.Should().Contain(AttendanceType.Register);
        attendanceTypeValues.Should().Contain(AttendanceType.Period);
        attendanceTypeValues.Should().Contain(AttendanceType.Adi);
        attendanceTypeValues.Should().Contain(AttendanceType.CheckInFromRegister);
        attendanceTypeValues.Should().Contain(AttendanceType.CheckInFromPeriod);
        attendanceTypeValues.Should().HaveCount(7);
    }

    [Theory]
    [InlineData("CheckIn", AttendanceType.CheckIn)]
    [InlineData("CheckOut", AttendanceType.CheckOut)]
    [InlineData("Register", AttendanceType.Register)]
    [InlineData("Period", AttendanceType.Period)]
    public void AttendanceType_ShouldParseFromString(string attendanceTypeString, AttendanceType expectedAttendanceType)
    {
        // Act
        var parsed = Enum.Parse<AttendanceType>(attendanceTypeString);

        // Assert
        parsed.Should().Be(expectedAttendanceType);
    }

    [Fact]
    public void Enum_TryParse_WithInvalidValue_ShouldReturnFalse()
    {
        // Act
        var success = Enum.TryParse<Gender>("InvalidGender", out var result);

        // Assert
        success.Should().BeFalse();
        result.Should().Be(default(Gender));
    }

    [Theory]
    [InlineData(Gender.None, "None")]
    [InlineData(Gender.Male, "Male")]
    [InlineData(Gender.Female, "Female")]
    public void Gender_ToString_ShouldReturnCorrectString(Gender gender, string expectedString)
    {
        // Act
        var result = gender.ToString();

        // Assert
        result.Should().Be(expectedString);
    }
}
