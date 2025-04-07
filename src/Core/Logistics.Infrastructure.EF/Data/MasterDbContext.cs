using Logistics.Domain.Entities;
using Logistics.Infrastructure.EF.Helpers;
using Logistics.Infrastructure.EF.Interceptors;
using Logistics.Infrastructure.EF.Options;
using Logistics.Shared.Consts;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.EF.Data;

public class MasterDbContext : IdentityDbContext<
    User, 
    AppRole, 
    string, 
    IdentityUserClaim<string>, 
    IdentityUserRole<string>, 
    IdentityUserLogin<string>, 
    AppRoleClaim, 
    IdentityUserToken<string>
    >, 
    IDataProtectionKeyContext
{
    private readonly DispatchDomainEventsInterceptor? _dispatchDomainEventsInterceptor;
    private readonly string _connectionString;
    private readonly ILogger<MasterDbContext>? _logger;

    public MasterDbContext(
        MasterDbContextOptions options,
        DispatchDomainEventsInterceptor? dispatchDomainEventsInterceptor = null,
        ILogger<MasterDbContext>? logger = null)
    {
        _dispatchDomainEventsInterceptor = dispatchDomainEventsInterceptor;
        _connectionString = options.ConnectionString ?? ConnectionStrings.LocalMaster;
        _logger = logger;
    }
    
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (_dispatchDomainEventsInterceptor is not null)
        {
            options.AddInterceptors(_dispatchDomainEventsInterceptor);
        }

        if (!options.IsConfigured)
        {
            DbContextHelpers.ConfigurePostgreSql(_connectionString, options);
            _logger?.LogInformation("Configured master database with connection string: {ConnectionString}", _connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Tenant>().ToTable("Tenants");

        builder.Entity<AppRole>(entity =>
        {
            entity.HasMany(i => i.Claims)
                .WithOne(i => i.Role)
                .HasForeignKey(i => i.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<User>(entity =>
        {
            entity.HasOne(i => i.Tenant)
                .WithMany(i => i.Users)
                .HasForeignKey(i => i.TenantId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        
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
            entity.ToTable("Subscriptions");
            
            entity.HasOne(i => i.Tenant)
                .WithOne(i => i.Subscription)
                .HasForeignKey<Subscription>(i => i.TenantId);

            entity.HasMany(i => i.Payments)
                .WithOne(i => i.Subscription)
                .HasForeignKey(i => i.SubscriptionId);
        });

        builder.Entity<PaymentMethod>(entity =>
        {
            entity.ToTable("PaymentMethods")
                .HasDiscriminator(pm => pm.Type)
                .HasValue<CardPaymentMethod>(PaymentMethodType.Card)
                .HasValue<UsBankAccountPaymentMethod>(PaymentMethodType.UsBankAccount)
                .HasValue<BankAccountPaymentMethod>(PaymentMethodType.InternationalBankAccount);

            entity.HasOne(i => i.Tenant)
                .WithMany(i => i.PaymentMethods)
                .HasForeignKey(i => i.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(i => i.StripePaymentMethodId);
            entity.HasIndex(i => i.TenantId);
        });
    }
}
