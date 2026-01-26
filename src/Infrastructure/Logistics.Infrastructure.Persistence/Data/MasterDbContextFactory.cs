using Logistics.Infrastructure.Persistence.Options;
using Microsoft.EntityFrameworkCore.Design;

namespace Logistics.Infrastructure.Persistence.Data;

public class MasterDbContextFactory : IDesignTimeDbContextFactory<MasterDbContext>
{
    public MasterDbContext CreateDbContext(string[] args)
    {
        return new MasterDbContext(new MasterDbContextOptions());
    }
}
