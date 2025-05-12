using Logistics.Domain.Entities;
using Logistics.Infrastructure.EF.Data.Configurations;
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
    private readonly DispatchDomainEventsInterceptor? _dispatchDomain;
    private readonly AuditableEntitySaveChangesInterceptor? _auditableEntity;
    private readonly string _connectionString;
    private readonly ILogger<MasterDbContext>? _logger;

    public MasterDbContext(
        MasterDbContextOptions options,
        DispatchDomainEventsInterceptor? dispatchDomain = null,
        AuditableEntitySaveChangesInterceptor? auditableEntity = null,
        ILogger<MasterDbContext>? logger = null)
    {
        _dispatchDomain = dispatchDomain;
        _auditableEntity = auditableEntity;
        _connectionString = options.ConnectionString ?? ConnectionStrings.LocalMaster;
        _logger = logger;
    }
    
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (_dispatchDomain is not null)
        {
            options.AddInterceptors(_dispatchDomain);
        }
        if (_auditableEntity is not null)
        {
            options.AddInterceptors(_auditableEntity);
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
        
        builder.ApplyConfiguration(new AuditableEntityConfiguration());
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
        });
    }
}
