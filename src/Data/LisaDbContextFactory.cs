using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Lisa.Data
{
    public class LisaDbContextFactory : IDesignTimeDbContextFactory<LisaDbContext>
    {
        public LisaDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<LisaDbContext>();
            var connectionString = configuration.GetConnectionString("Lisa");
            optionsBuilder.UseNpgsql(connectionString);

            return new LisaDbContext(optionsBuilder.Options);
        }
    }
}
