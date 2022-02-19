using Microsoft.EntityFrameworkCore;

namespace Logistics.DbMigrator;

public static class SeedData
{
    public static async Task InitializeAsync<T>(T databaseContext)
        where T : DbContext
    {
        await MigrateDatabaseAsync<T>(databaseContext);
    }

    private static async Task MigrateDatabaseAsync<T>(T databaseContext)
        where T : DbContext
    {
        try
        {
            await databaseContext.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Thrown exception in SeedData.MigrateDatabaseAsync(): " + ex);
        }
    }
}
