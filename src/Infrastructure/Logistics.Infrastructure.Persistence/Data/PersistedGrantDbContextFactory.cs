using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Logistics.Infrastructure.Persistence.Data;

public class PersistedGrantDbContextFactory : IDesignTimeDbContextFactory<PersistedGrantDbContext>
{
    public PersistedGrantDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<PersistedGrantDbContext>();
        DuendeOperationalStore.ConfigureDbContext(builder, null);
        return new PersistedGrantDbContext(builder.Options)
        {
            StoreOptions = DuendeOperationalStore.ConfigureStoreOptions(new OperationalStoreOptions())
        };
    }
}
