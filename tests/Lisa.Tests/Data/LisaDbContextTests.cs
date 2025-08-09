using Lisa.Tests.Helpers;

namespace Lisa.Tests.Data;

public class LisaDbContextTests : TestBase
{
    [Fact]
    public async Task DbContext_ShouldCreateAndSaveSchool()
    {
        // Arrange
        var school = new School
        {
            Id = Guid.NewGuid(),
            ShortName = "TS",
            LongName = "Test School",
            Color = "#FF0000",
            SmtpHost = "smtp.test.com",
            SmtpPort = 587,
            SmtpEmail = "test@test.com",
            SmtpPassword = "password",
            SmtpUsername = "testuser",
            FromEmail = "noreply@test.com"
        };

        // Act
        DbContext.Schools.Add(school);
        var result = await DbContext.SaveChangesAsync();

        // Assert
        result.Should().Be(1);
        
        var savedSchool = await DbContext.Schools.FindAsync(school.Id);
        savedSchool.Should().NotBeNull();
        savedSchool!.ShortName.Should().Be("TS");
        savedSchool.LongName.Should().Be("Test School");
    }

    [Fact]
    public async Task DbContext_ShouldCreateAndSaveUser()
    {
        // Arrange
        await SeedDatabaseAsync();
        var school = await DbContext.Schools.FirstAsync();
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = "newuser@test.com",
            Email = "newuser@test.com",
            Name = "New User",
            Surname = "Test",
            SchoolId = school.Id
        };

        // Act
        DbContext.Users.Add(user);
        var result = await DbContext.SaveChangesAsync();

        // Assert
        result.Should().Be(1);
        
        var savedUser = await DbContext.Users.FindAsync(user.Id);
        savedUser.Should().NotBeNull();
        savedUser!.Name.Should().Be("New User");
        savedUser.SchoolId.Should().Be(school.Id);
    }

    [Fact]
    public async Task DbContext_ShouldHandleUserSchoolRelationship()
    {
        // Arrange
        await SeedDatabaseAsync();

        // Act
        var userWithSchool = await DbContext.Users
            .Include(u => u.School)
            .FirstAsync();

        // Assert
        userWithSchool.Should().NotBeNull();
        userWithSchool.School.Should().NotBeNull();
        userWithSchool.SchoolId.Should().Be(userWithSchool.School!.Id);
    }

    [Fact]
    public async Task DbContext_ShouldQuerySchoolsWithRelatedData()
    {
        // Arrange
        await SeedDatabaseAsync();

        // Act
        var schools = await DbContext.Schools.ToListAsync();

        // Assert
        schools.Should().NotBeEmpty();
        schools.First().Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void DbContext_ShouldHaveCorrectDbSets()
    {
        // Assert
        DbContext.Schools.Should().NotBeNull();
        DbContext.Users.Should().NotBeNull();
        
        // Verify other expected DbSets exist
        var properties = typeof(LisaDbContext).GetProperties()
            .Where(p => p.PropertyType.IsGenericType && 
                       p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(p => p.Name)
            .ToList();

        properties.Should().Contain("Schools");
        properties.Should().Contain("Users");
    }
}
