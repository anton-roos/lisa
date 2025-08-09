using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lisa.Tests.Integration;

/// <summary>
/// Base class for integration tests that provides a configured web application factory
/// </summary>
public class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;

    public IntegrationTestBase(WebApplicationFactory<Program> factory)
    {
        Factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the original DbContextFactory registration that uses PostgreSQL
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDbContextFactory<LisaDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add a simple DbContextFactory for InMemory testing
                services.AddDbContextFactory<LisaDbContext>(options =>
                    options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));
            });
        });

        Client = Factory.CreateClient();
    }

    /// <summary>
    /// Gets a scoped service from the test application
    /// </summary>
    protected T GetService<T>() where T : notnull
    {
        using var scope = Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<T>();
    }

    /// <summary>
    /// Executes an action within a database context scope
    /// </summary>
    protected async Task WithDbContextAsync(Func<LisaDbContext, Task> action)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<LisaDbContext>>();
        using var context = dbContextFactory.CreateDbContext();
        await action(context);
    }

    /// <summary>
    /// Seeds the test database with basic data
    /// </summary>
    protected async Task SeedTestDataAsync()
    {
        await WithDbContextAsync(async context =>
        {
            if (!await context.Schools.AnyAsync())
            {
                var school = new School
                {
                    Id = Guid.NewGuid(),
                    ShortName = "TS",
                    LongName = "Test School",
                    SmtpHost = "smtp.test.com",
                    SmtpPort = 587,
                    SmtpEmail = "test@test.com",
                    SchoolTypeId = Guid.NewGuid(),
                    SchoolCurriculumId = Guid.NewGuid()
                };

                context.Schools.Add(school);
                await context.SaveChangesAsync();
            }
        });
    }

    public void Dispose()
    {
        Client?.Dispose();
        Factory?.Dispose();
        GC.SuppressFinalize(this);
    }
}
