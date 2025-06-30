namespace Lisa.Application.FunctionalTests;

public static class TestDatabaseFactory
{
    public static async Task<ITestDatabase> CreateAsync()
    {
        // Testcontainers requires Docker. To use a local PostgreSQL database instead,
        // switch to `PostgreSqlTestDatabase` and update appsettings.json.
        var database = new PostgreSqlTestcontainersTestDatabase();

        await database.InitialiseAsync();

        return database;
    }
}
