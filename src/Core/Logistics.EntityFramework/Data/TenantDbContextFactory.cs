using Microsoft.EntityFrameworkCore.Design;

namespace Logistics.EntityFramework.Data;

public class TenantDbContextFactory : IDesignTimeDbContextFactory<TenantDbContext>
{
    public TenantDbContext CreateDbContext(string[] args)
    {
        return new TenantDbContext(new TenantDbContextOptions(), null);
    }
}