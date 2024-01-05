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
        builder.Entity<Tenant>().ToTable("Tenants");
        
        builder.Entity<SubscriptionPayment>(entity =>
        {
            entity.ToTable("SubscriptionPayments");
            entity.Property(i => i.Amount).HasPrecision(18, 2);
        });
        
        builder.Entity<SubscriptionPlan>(entity =>
        {
            entity.ToTable("SubscriptionPlans");
            entity.Property(i => i.Price).HasPrecision(18, 2);

            entity.HasMany(i => i.Subscriptions)
                .WithOne(i => i.Plan)
                .HasForeignKey(i => i.PlanId);
        });

        builder.Entity<Subscription>(entity =>
        {
            entity.ToTable("Subscription");
            
            entity.HasOne(i => i.Tenant)
                .WithOne(i => i.Subscription)
                .HasForeignKey<Subscription>(i => i.TenantId);

            entity.HasMany(i => i.Payments)
                .WithOne(i => i.Subscription)
                .HasForeignKey(i => i.SubscriptionId);
        });
    }
}
