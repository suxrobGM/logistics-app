using System.Text.Json;
using Logistics.Domain.Entities;
using Logistics.Infrastructure.Data.Extensions;
using Logistics.Infrastructure.Helpers;
using Logistics.Infrastructure.Interceptors;
using Logistics.Infrastructure.Options;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.Data;

public class TenantDbContext : DbContext
{
    private readonly AuditableEntitySaveChangesInterceptor? _auditableEntity;

    // Default fallback connection string for local development and testing
    private readonly string _defaultConnectionString;

    private readonly DispatchDomainEventsInterceptor? _dispatchDomain;
    private readonly ILogger<TenantDbContext>? _logger;

    public TenantDbContext(
        TenantDbContextOptions? tenantDbContextOptions = null,
        DispatchDomainEventsInterceptor? dispatchDomain = null,
        AuditableEntitySaveChangesInterceptor? auditableEntity = null,
        ILogger<TenantDbContext>? logger = null)
    {
        _dispatchDomain = dispatchDomain;
        _auditableEntity = auditableEntity;
        _logger = logger;

        _defaultConnectionString = tenantDbContextOptions?.ConnectionString
                                   ?? ConnectionStrings.LocalDefaultTenant;
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
        _logger?.LogInformation("Switched tenant database to '{TenantName}'.", tenant.Name);
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
            _logger?.LogInformation("Configured tenant database with default connection string.");
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Scan and apply all configurations from the /Data/Configurations folder
        // for entities implementing ITenantEntity
        builder.ApplyTenantConfigurationsFromAssemblyContaining<TenantDbContext>();

        builder.Entity<CompanyStatsDto>(entity =>
        {
            entity.HasNoKey();
            entity.ToView(null);
        });

        builder.Entity<TruckStatsDto>(entity =>
        {
            entity.HasNoKey();
            entity.ToView(null);

            entity.Property(t => t.Drivers)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<EmployeeDto>>(v, (JsonSerializerOptions)null));
        });
    }
}
