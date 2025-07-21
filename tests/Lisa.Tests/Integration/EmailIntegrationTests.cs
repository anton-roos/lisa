using Lisa.Tests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lisa.Tests.Integration;

public class EmailIntegrationTests : TestBase, IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public EmailIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace the database context with in-memory database
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<LisaDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<LisaDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });
            });
        });
    }

    [Fact]
    public void EmailService_ShouldBeRegisteredInDI()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();

        // Act
        var emailService = scope.ServiceProvider.GetService<EmailService>();

        // Assert
        emailService.Should().NotBeNull();
    }

    [Fact]
    public void SchoolService_ShouldBeRegisteredInDI()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();

        // Act
        var schoolService = scope.ServiceProvider.GetService<SchoolService>();

        // Assert
        schoolService.Should().NotBeNull();
    }

    [Fact]
    public void UserService_ShouldBeRegisteredInDI()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();

        // Act
        var userService = scope.ServiceProvider.GetService<UserService>();

        // Assert
        userService.Should().NotBeNull();
    }

    [Fact]
    public void DbContext_ShouldBeAccessible()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();

        // Act
        var dbContext = scope.ServiceProvider.GetService<LisaDbContext>();

        // Assert
        dbContext.Should().NotBeNull();
    }
}
