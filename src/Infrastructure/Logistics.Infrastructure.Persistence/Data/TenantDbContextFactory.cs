using Microsoft.EntityFrameworkCore.Design;

namespace Logistics.Infrastructure.Persistence.Data;

public class TenantDbContextFactory : IDesignTimeDbContextFactory<TenantDbContext>
{
    public TenantDbContext CreateDbContext(string[] args) => new();
}
