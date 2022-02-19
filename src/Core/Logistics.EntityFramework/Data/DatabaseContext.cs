using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Interfaces;
using Logistics.Domain.Entities;

namespace Logistics.EntityFramework.Data;

public class DatabaseContext : IdentityDbContext<User, UserRole, string>, IPersistedGrantDbContext
{
    private string connectionString;

    public DatabaseContext(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public DatabaseContext(DbContextOptions options)
        : base(options)
    {
        connectionString = "Server=localhost; Database=LogisticsDB; Trusted_Connection=True";
    }
    
    public DbSet<PersistedGrant>? PersistedGrants { get; set; }
    public DbSet<DeviceFlowCodes>? DeviceFlowCodes { get; set; }
    
    Task<int> IPersistedGrantDbContext.SaveChangesAsync() => base.SaveChangesAsync();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(connectionString)
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
        optionsBuilder.UseSqlServer("Server=localhost; Database=LogisticsDB; Trusted_Connection=True")
            .UseLazyLoadingProxies();

        return new DatabaseContext(optionsBuilder.Options);
    }
}