using Microsoft.EntityFrameworkCore.Design;

namespace Logistics.EntityFramework.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<MainDbContext>
{
    public MainDbContext CreateDbContext(string[] args)
    {
        return new MainDbContext(new MainDbContextOptions());
    }
}