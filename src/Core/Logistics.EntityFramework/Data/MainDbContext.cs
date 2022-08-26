using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Logistics.EntityFramework.Helpers;
using Microsoft.AspNetCore.Identity;

namespace Logistics.EntityFramework.Data;

public class MainDbContext : IdentityDbContext<User, AppRole, string>
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
        });
        
        builder.Entity<AppRole>(entity =>
        {
            entity.ToTable("roles");
        });
        
        builder.Entity<IdentityUserRole<string>>(entity =>
        {
            entity.ToTable("user_roles");
        });
    }
}
