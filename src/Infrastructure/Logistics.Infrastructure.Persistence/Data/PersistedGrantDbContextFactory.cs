using Open.IdentityServer.EntityFramework.DbContexts;
using Open.IdentityServer.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Logistics.Infrastructure.Persistence.Data;

public class PersistedGrantDbContextFactory : IDesignTimeDbContextFactory<PersistedGrantDbContext>
{
    public PersistedGrantDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<PersistedGrantDbContext>();
        OperationalStoreSetup.ConfigureDbContext(builder, null);
        return new PersistedGrantDbContext(
            builder.Options,
            OperationalStoreSetup.ConfigureStoreOptions(new OperationalStoreOptions()));
    }
}
