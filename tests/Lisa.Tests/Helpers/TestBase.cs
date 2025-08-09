using Microsoft.Extensions.Logging;

namespace Lisa.Tests.Helpers;

public class TestDbContextFactory : IDbContextFactory<LisaDbContext>
{
    private readonly DbContextOptions<LisaDbContext> _options;

    public TestDbContextFactory()
    {
        _options = new DbContextOptionsBuilder<LisaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    public LisaDbContext CreateDbContext()
    {
        var logger = new FakeLogger<LisaDbContext>();
        return new LisaDbContext(_options, logger);
    }

    public async Task<LisaDbContext> CreateDbContextAsync()
    {
        return await Task.FromResult(CreateDbContext());
    }
}

public class TestBase : IDisposable
{
    protected readonly TestDbContextFactory DbContextFactory;
    protected readonly LisaDbContext DbContext;
    protected readonly FakeLogger<object> FakeLogger;

    public TestBase()
    {
        DbContextFactory = new TestDbContextFactory();
        DbContext = DbContextFactory.CreateDbContext();
        FakeLogger = new FakeLogger<object>();
    }

    protected async Task SeedDatabaseAsync()
    {
        // Create test schools
        var testSchool = new School
        {
            Id = Guid.NewGuid(),
            ShortName = "TS",
            LongName = "Test School",
            SmtpHost = "smtp.test.com",
            SmtpPort = 587,
            SmtpUsername = "test@test.com",
            SmtpPassword = "password",
            SmtpEmail = "test@test.com",
            FromEmail = "noreply@test.com",
            SchoolTypeId = Guid.NewGuid(),
            SchoolCurriculumId = Guid.NewGuid()
        };

        DbContext.Schools.Add(testSchool);

        // Create test users
        var testUser = new User
        {
            Id = Guid.NewGuid(),
            UserName = "testuser@test.com",
            Email = "testuser@test.com",
            Name = "Test User",
            Surname = "User",
            SchoolId = testSchool.Id
        };

        DbContext.Users.Add(testUser);

        await DbContext.SaveChangesAsync();
    }

    public void Dispose()
    {
        DbContext?.Dispose();
    }
}
