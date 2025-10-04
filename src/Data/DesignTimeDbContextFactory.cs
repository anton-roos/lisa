using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Lisa.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<LisaDbContext>
{
    public LisaDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        // Get connection string from configuration
        var connectionString = configuration.GetConnectionString("Lisa");

        // Create logger factory for design time
        var loggerFactory = LoggerFactory.Create(builder =>
            builder.AddConsole());

        // Create logger for context
        var logger = loggerFactory.CreateLogger<LisaDbContext>();

        // Create DbContextOptions with Npgsql provider
        var optionsBuilder = new DbContextOptionsBuilder<LisaDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new LisaDbContext(optionsBuilder.Options);
    }
}