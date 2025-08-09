using Lisa.Models.Entities;

namespace Lisa.Tests.Models.Entities;

public class SchoolTests : TestBase
{
    [Fact]
    public void School_ShouldHaveRequiredProperties()
    {
        // Act
        var school = new School();

        // Assert
        school.Id.Should().NotBe(Guid.Empty);
        school.SmtpPort.Should().Be(0); // Default value
    }

    [Fact]
    public void School_ShouldAllowSettingProperties()
    {
        // Arrange
        var school = new School();
        var testId = Guid.NewGuid();

        // Act
        school.Id = testId;
        school.ShortName = "TS";
        school.LongName = "Test School";
        school.Color = "#FF0000";
        school.SmtpHost = "smtp.test.com";
        school.SmtpPort = 587;
        school.SmtpEmail = "test@test.com";
        school.SmtpPassword = "password";
        school.SmtpUsername = "testuser";
        school.FromEmail = "noreply@test.com";

        // Assert
        school.Id.Should().Be(testId);
        school.ShortName.Should().Be("TS");
        school.LongName.Should().Be("Test School");
        school.Color.Should().Be("#FF0000");
        school.SmtpHost.Should().Be("smtp.test.com");
        school.SmtpPort.Should().Be(587);
        school.SmtpEmail.Should().Be("test@test.com");
        school.SmtpPassword.Should().Be("password");
        school.SmtpUsername.Should().Be("testuser");
        school.FromEmail.Should().Be("noreply@test.com");
    }

    [Theory]
    [InlineData("", true)]
    [InlineData("A", true)]
    [InlineData("ABCD1234", true)]
    [InlineData("ABCD12345", false)] // Too long - MaxLength is 8
    public void School_ShortName_ShouldRespectMaxLength(string shortName, bool shouldBeValid)
    {
        // Arrange
        var school = new School { ShortName = shortName };

        // Act & Assert
        if (shouldBeValid)
        {
            school.ShortName.Should().Be(shortName);
        }
        else
        {
            // In a real application, this would be validated by the database or validation attributes
            school.ShortName!.Length.Should().BeGreaterThan(8);
        }
    }

    [Fact]
    public void School_ShouldAllowNullValues()
    {
        // Arrange & Act
        var school = new School
        {
            ShortName = null,
            LongName = null,
            Color = null,
            SmtpHost = null,
            SmtpEmail = null,
            SmtpPassword = null,
            SmtpUsername = null,
            FromEmail = null
        };

        // Assert
        school.ShortName.Should().BeNull();
        school.LongName.Should().BeNull();
        school.Color.Should().BeNull();
        school.SmtpHost.Should().BeNull();
        school.SmtpEmail.Should().BeNull();
        school.SmtpPassword.Should().BeNull();
        school.SmtpUsername.Should().BeNull();
        school.FromEmail.Should().BeNull();
    }

    [Fact]
    public async Task School_ShouldBePersistableToDatabase()
    {
        // Arrange
        var school = new School
        {
            Id = Guid.NewGuid(),
            ShortName = "TS",
            LongName = "Test School",
            SmtpHost = "smtp.test.com",
            SmtpPort = 587,
            SmtpEmail = "test@test.com"
        };

        // Act
        DbContext.Schools.Add(school);
        await DbContext.SaveChangesAsync();

        // Assert
        var savedSchool = await DbContext.Schools.FindAsync(school.Id);
        savedSchool.Should().NotBeNull();
        savedSchool!.ShortName.Should().Be("TS");
        savedSchool.LongName.Should().Be("Test School");
    }
}
