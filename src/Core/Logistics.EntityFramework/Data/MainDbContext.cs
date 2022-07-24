using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Logistics.EntityFramework.Helpers;

namespace Logistics.EntityFramework.Data;

public class MainDbContext : IdentityDbContext<User>
{
    private readonly string _connectionString;

    public MainDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public MainDbContext(DbContextOptions<MainDbContext> options)
        : base(options)
    {
        _connectionString = ConnectionStrings.LocalDefaultTenant;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
        {
            DbContextHelpers.ConfigureMySql(_connectionString, options);
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Tenant>(entity =>
        {
            entity.ToTable("tenants");
        });

        builder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.OwnsOne(m => m.Role);
        });
    }
}
