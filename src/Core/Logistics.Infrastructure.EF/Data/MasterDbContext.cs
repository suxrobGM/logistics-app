using Logistics.Domain.Entities;
using Logistics.Infrastructure.EF.Helpers;
using Logistics.Infrastructure.EF.Interceptors;
using Logistics.Infrastructure.EF.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Infrastructure.EF.Data;

public class MasterDbContext : IdentityDbContext<User, AppRole, string>
{
    private readonly DispatchDomainEventsInterceptor? _dispatchDomainEventsInterceptor;
    private readonly string _connectionString;

    public MasterDbContext(
        MasterDbContextOptions options,
        DispatchDomainEventsInterceptor? dispatchDomainEventsInterceptor)
    {
        _dispatchDomainEventsInterceptor = dispatchDomainEventsInterceptor;
        _connectionString = options.ConnectionString ?? ConnectionStrings.LocalMaster;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
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
        builder.Entity<User>().ToTable("Users");
        builder.Entity<SubscriptionPlan>().ToTable("Subscriptions");
        builder.Entity<Tenant>(entity =>
        {
            entity.ToTable("Tenants");
        });
    }
}
