using Microsoft.EntityFrameworkCore.Design;

namespace Logistics.EntityFramework.Data;

public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(string[] args)
    {
        var connectionString = "Server=localhost; Database=LogisticsDB; Uid=root; Pwd=Suxrobbek0729; Connect Timeout=10";
        return new DatabaseContext(connectionString);
    }
}