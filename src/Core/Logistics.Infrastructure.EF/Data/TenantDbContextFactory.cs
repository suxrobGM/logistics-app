using Logistics.Infrastructure.EF.Options;
using Microsoft.EntityFrameworkCore.Design;

namespace Logistics.Infrastructure.EF.Data;

public class TenantDbContextFactory : IDesignTimeDbContextFactory<TenantDbContext>
{
    public TenantDbContext CreateDbContext(string[] args)
    {
        return new TenantDbContext(new TenantDbContextOptions(), null, null);
    }
}
