using Logistics.Infrastructure.EF.Options;
using Microsoft.EntityFrameworkCore.Design;

namespace Logistics.Infrastructure.EF.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<MainDbContext>
{
    public MainDbContext CreateDbContext(string[] args)
    {
        return new MainDbContext(new MainDbContextOptions());
    }
}