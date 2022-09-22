using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Logistics.EntityFramework.Data;

public class MainDbContext : IdentityDbContext<User, AppRole, string>
{
    private readonly string _connectionString;

    public MainDbContext(MainDbContextOptions options)
    {
        _connectionString = options.ConnectionString ?? ConnectionStrings.LocalMain;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (options.IsConfigured)
            return;
        
        DbContextHelpers.ConfigureMySql(_connectionString, options);
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
