using Logistics.Infrastructure.EF.Interceptors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Logistics.Infrastructure.EF.Data;

public class MainDbContext : IdentityDbContext<User, AppRole, string>
{
    private readonly AuditableEntitySaveChangesInterceptor? _auditableEntitySaveChangesInterceptor;
    private readonly DispatchDomainEventsInterceptor? _dispatchDomainEventsInterceptor;
    private readonly string _connectionString;

    public MainDbContext(
        MainDbContextOptions options,
        AuditableEntitySaveChangesInterceptor? auditableEntitySaveChangesInterceptor,
        DispatchDomainEventsInterceptor? dispatchDomainEventsInterceptor)
    {
        _auditableEntitySaveChangesInterceptor = auditableEntitySaveChangesInterceptor;
        _dispatchDomainEventsInterceptor = dispatchDomainEventsInterceptor;
        _connectionString = options.ConnectionString ?? ConnectionStrings.LocalMain;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (_auditableEntitySaveChangesInterceptor is not null)
        {
            options.AddInterceptors(_auditableEntitySaveChangesInterceptor);
        }
        if (_dispatchDomainEventsInterceptor is not null)
        {
            options.AddInterceptors(_dispatchDomainEventsInterceptor);
        }

        if (!options.IsConfigured)
        {
            DbContextHelpers.ConfigureSqlServer(_connectionString, options);
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<AppRole>().ToTable("Roles");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
        builder.Entity<Tenant>().ToTable("Tenants");
        builder.Entity<User>().ToTable("Users");

        // builder.Entity<User>(entity =>
        // {
        //     entity.ToTable("Users");
        //     entity.Property(e => e.JoinedTenantIds)
        //         .HasConversion(
        //             v => string.Join(',', v),
        //             v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToHashSet());
        // });
    }
}
