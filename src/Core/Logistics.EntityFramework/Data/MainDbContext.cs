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
        
        builder.Entity<AppRole>().ToTable("roles");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("role_claims");
        builder.Entity<IdentityUserRole<string>>().ToTable("user_roles");
        builder.Entity<IdentityUserLogin<string>>().ToTable("user_logins");
        builder.Entity<IdentityUserClaim<string>>().ToTable("user_claims");
        builder.Entity<IdentityUserToken<string>>().ToTable("user_tokens");
        builder.Entity<Tenant>().ToTable("tenants");
        builder.Entity<User>().ToTable("users");
    }
}
