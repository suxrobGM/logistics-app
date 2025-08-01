using Logistics.Infrastructure.Options;
using Microsoft.EntityFrameworkCore.Design;

namespace Logistics.Infrastructure.Data;

public class MasterDbContextFactory : IDesignTimeDbContextFactory<MasterDbContext>
{
    public MasterDbContext CreateDbContext(string[] args)
    {
        return new MasterDbContext(new MasterDbContextOptions());
    }
}
