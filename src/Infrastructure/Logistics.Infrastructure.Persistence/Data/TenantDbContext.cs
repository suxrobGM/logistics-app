using System.Text.Json;
using Logistics.Domain.Entities;
using Logistics.Infrastructure.Persistence.Conventions;
using Logistics.Infrastructure.Persistence.Extensions;
using Logistics.Infrastructure.Persistence.Helpers;
using Logistics.Infrastructure.Persistence.Interceptors;
using Logistics.Infrastructure.Persistence.Options;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.Persistence.Data;

public class TenantDbContext : DbContext
{
    private readonly AuditableEntitySaveChangesInterceptor? _auditableEntity;

    // Default fallback connection string for local development and testing
    private readonly string _defaultConnectionString;

    private readonly DispatchDomainEventsInterceptor? _dispatchDomain;
    private readonly IDataProtectionProvider? _dataProtectionProvider;
    private readonly ILogger<TenantDbContext>? _logger;

    public TenantDbContext(
        TenantDbContextOptions? tenantDbContextOptions = null,
        DispatchDomainEventsInterceptor? dispatchDomain = null,
        AuditableEntitySaveChangesInterceptor? auditableEntity = null,
        IDataProtectionProvider? dataProtectionProvider = null,
        ILogger<TenantDbContext>? logger = null)
    {
        _dispatchDomain = dispatchDomain;
        _auditableEntity = auditableEntity;
        _dataProtectionProvider = dataProtectionProvider;
        _logger = logger;

        _defaultConnectionString = tenantDbContextOptions?.ConnectionString
                                  ?? ConnectionStrings.LocalDefaultTenant;

        NavigationDiscoveryGuard.Attach(ChangeTracker);
    }

    /// <summary>
    ///     Switch the underlying connection to a tenant-specific database.
    ///     Call this BEFORE the first query or SaveChanges.
    /// </summary>
    /// <param name="tenant">The tenant to switch to, containing the connection string. </param>
    public void SwitchToTenant(Tenant tenant)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenant.ConnectionString);
        Database.SetConnectionString(tenant.ConnectionString); // EF Core runtime retargeting
        _logger?.LogDebug("Switched tenant database to '{TenantName}'.", tenant.Name);
    }

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
            DbContextHelpers.ConfigurePostgreSql(_defaultConnectionString, options);
        }
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Conventions.Add(_ => new SnakeCaseEnumConvention());
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Scan and apply all configurations from the /Data/Configurations folder
        // for entities implementing ITenantEntity
        builder.ApplyTenantConfigurationsFromAssembly<TenantDbContext>();

        // Prune entity types that are only relevant for the master database
        // It avoids issues with master-specific entities being included in the tenant context and migration errors
        builder.PruneMasterOnlyTypesForTenant();

        // Encrypt provider-secret columns at rest (ELD / LoadBoard / Accounting). Applied after
        // entity configs so it overrides their plain string mappings. No-op when no protector.
        builder.ApplyEncryptedSecretColumns(_dataProtectionProvider);

        // Query-only DTOs for PostgreSQL functions - no table generation
        builder.Entity<CompanyStatsDto>(entity =>
        {
            entity.HasNoKey();
            entity.ToTable((string?)null);
        });

        builder.Entity<TruckStatsDto>(entity =>
        {
            entity.HasNoKey();
            entity.ToTable((string?)null);

            entity.Property(t => t.Drivers)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<EmployeeDto>>(v, (JsonSerializerOptions)null));
        });
    }
}
