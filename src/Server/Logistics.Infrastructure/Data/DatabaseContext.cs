using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Options;
using Logistics.Domain.UserAggregate;

namespace Logistics.Infrastructure.Data;

public class DatabaseContext : IdentityDbContext<User, UserRole, string>, IPersistedGrantDbContext
{
    private readonly IOptions<OperationalStoreOptions> _operationalStoreOptions;

    public DatabaseContext(
        DbContextOptions options,
        IOptions<OperationalStoreOptions> operationalStoreOptions)
        : base(options)
    {
        _operationalStoreOptions = operationalStoreOptions;
    }
    
    public DbSet<PersistedGrant> PersistedGrants { get; set; }
    public DbSet<DeviceFlowCodes> DeviceFlowCodes { get; set; }
    
    Task<int> IPersistedGrantDbContext.SaveChangesAsync() => base.SaveChangesAsync();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=localhost; Initial Catalog=LogisticsDB; Trusted_Connection=True")
                .UseLazyLoadingProxies();
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}

public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
        optionsBuilder.UseSqlServer("Server=localhost; Initial Catalog=LogisticsDB; Trusted_Connection=True")
            .UseLazyLoadingProxies();

        return new DatabaseContext(optionsBuilder.Options, new OptionsWrapper<OperationalStoreOptions>(new OperationalStoreOptions()));
    }
}