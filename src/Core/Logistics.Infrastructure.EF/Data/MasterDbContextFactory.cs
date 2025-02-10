using Logistics.Infrastructure.EF.Options;
using Microsoft.EntityFrameworkCore.Design;

namespace Logistics.Infrastructure.EF.Data;

public class MasterDbContextFactory : IDesignTimeDbContextFactory<MasterDbContext>
{
    public MasterDbContext CreateDbContext(string[] args)
    {
        return new MasterDbContext(new MasterDbContextOptions());
    }
}
