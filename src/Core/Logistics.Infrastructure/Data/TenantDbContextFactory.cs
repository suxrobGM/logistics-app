using Logistics.Infrastructure.Options;
using Microsoft.EntityFrameworkCore.Design;

namespace Logistics.Infrastructure.Data;

public class TenantDbContextFactory : IDesignTimeDbContextFactory<TenantDbContext>
{
    public TenantDbContext CreateDbContext(string[] args)
    {
        return new TenantDbContext();
    }
}
