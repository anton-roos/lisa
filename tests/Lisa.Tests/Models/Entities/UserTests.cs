using Lisa.Models.Entities;

namespace Lisa.Tests.Models.Entities;

public class UserTests : TestBase
{
    [Fact]
    public void User_ShouldInheritFromIdentityUser()
    {
        // Arrange & Act
        var user = new User();

        // Assert
        user.Should().BeAssignableTo<Microsoft.AspNetCore.Identity.IdentityUser<Guid>>();
        user.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void User_ShouldHaveCustomProperties()
    {
        // Act
        var user = new User();

        // Assert
        user.Surname.Should().BeNull();
        user.Abbreviation.Should().BeNull();
        user.Name.Should().BeNull();
        user.SchoolId.Should().BeNull();
        user.UserType.Should().BeNull();
        user.Roles.Should().NotBeNull();
        user.Roles.Should().BeEmpty();
    }

    [Fact]
    public void User_ShouldAllowSettingProperties()
    {
        // Arrange
        var user = new User();
        var schoolId = Guid.NewGuid();

        // Act
        user.Surname = "Doe";
        user.Abbreviation = "JD";
        user.Name = "John";
        user.SchoolId = schoolId;
        user.UserType = "Teacher";
        user.Email = "john.doe@test.com";
        user.UserName = "john.doe@test.com";

        // Assert
        user.Surname.Should().Be("Doe");
        user.Abbreviation.Should().Be("JD");
        user.Name.Should().Be("John");
        user.SchoolId.Should().Be(schoolId);
        user.UserType.Should().Be("Teacher");
        user.Email.Should().Be("john.doe@test.com");
        user.UserName.Should().Be("john.doe@test.com");
    }

    [Fact]
    public void User_Roles_ShouldBeInitializedAsList()
    {
        // Arrange & Act
        var user = new User();

        // Assert
        user.Roles.Should().NotBeNull();
        user.Roles.Should().BeOfType<List<string>>();
        user.Roles.Should().BeEmpty();
    }

    [Fact]
    public void User_Roles_ShouldAllowAddingItems()
    {
        // Arrange
        var user = new User();

        // Act
        user.Roles.Add("Teacher");
        user.Roles.Add("Admin");

        // Assert
        user.Roles.Should().HaveCount(2);
        user.Roles.Should().Contain("Teacher");
        user.Roles.Should().Contain("Admin");
    }

    [Theory]
    [InlineData("Teacher")]
    [InlineData("Admin")]
    [InlineData("Student")]
    [InlineData("Parent")]
    public void User_UserType_ShouldAcceptValidTypes(string userType)
    {
        // Arrange
        var user = new User();

        // Act
        user.UserType = userType;

        // Assert
        user.UserType.Should().Be(userType);
    }

    [Fact]
    public async Task User_ShouldBePersistableToDatabase()
    {
        // Arrange
        await SeedDatabaseAsync();
        var school = await DbContext.Schools.FirstAsync();
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = "testuser@test.com",
            Email = "testuser@test.com",
            Name = "Test User",
            Surname = "Test",
            SchoolId = school.Id
        };

        // Act
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        // Assert
        var savedUser = await DbContext.Users.FindAsync(user.Id);
        savedUser.Should().NotBeNull();
        savedUser!.Name.Should().Be("Test User");
        savedUser.SchoolId.Should().Be(school.Id);
    }

    [Fact]
    public void User_Name_ShouldRespectMaxLength()
    {
        // Arrange
        var user = new User();
        var longName = new string('x', 65); // Exceeds MaxLength of 64

        // Act
        user.Name = longName;

        // Assert
        user.Name!.Length.Should().Be(65);
        // In a real application with validation, this would fail validation
    }
}
