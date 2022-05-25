using Microsoft.EntityFrameworkCore;

namespace Logistics.DbMigrator;

public static class SeedData
{
    public static async Task<bool> InitializeAsync<T>(T databaseContext)
        where T : DbContext
    {
        try
        {
            await MigrateDatabaseAsync(databaseContext);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
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
            throw;
        }
    }
}
