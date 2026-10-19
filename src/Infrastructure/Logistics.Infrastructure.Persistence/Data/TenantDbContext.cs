using Logistics.Domain.Entities;
using Logistics.Domain.Options;
using Logistics.Infrastructure.Persistence.Conventions;
using Logistics.Infrastructure.Persistence.Extensions;
using Logistics.Infrastructure.Persistence.Helpers;
using Logistics.Infrastructure.Persistence.Interceptors;
using Logistics.Infrastructure.Persistence.Options;
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
    private readonly TenantDatabaseDefaults? _databaseDefaults;
    private readonly ILogger<TenantDbContext>? _logger;

    public TenantDbContext(
        TenantDbContextOptions? tenantDbContextOptions = null,
        DispatchDomainEventsInterceptor? dispatchDomain = null,
        AuditableEntitySaveChangesInterceptor? auditableEntity = null,
        IDataProtectionProvider? dataProtectionProvider = null,
        TenantDatabaseDefaults? databaseDefaults = null,
        ILogger<TenantDbContext>? logger = null)
    {
        _dispatchDomain = dispatchDomain;
        _auditableEntity = auditableEntity;
        _dataProtectionProvider = dataProtectionProvider;
        _databaseDefaults = databaseDefaults;
        _logger = logger;

        _defaultConnectionString = tenantDbContextOptions?.ConnectionString
                                  ?? ConnectionStrings.LocalDefaultTenant;

        NavigationDiscoveryGuard.Attach(ChangeTracker);
    }

    /// <summary>
    ///     Switch the underlying connection to a tenant-specific database.
    ///     Call this BEFORE the first query or SaveChanges.
    /// </summary>
    /// <param name="tenant">The tenant to switch to; its database is reached with the configured defaults.</param>
    public void SwitchToTenant(Tenant tenant)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenant.DatabaseName);

        if (_databaseDefaults is null)
        {
            throw new InvalidOperationException(
                $"The '{TenantDatabaseDefaults.SectionName}' configuration section is required to reach tenant databases.");
        }

        Database.SetConnectionString(_databaseDefaults.BuildConnectionString(tenant.DatabaseName)); // EF Core runtime retargeting
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
    }
}
