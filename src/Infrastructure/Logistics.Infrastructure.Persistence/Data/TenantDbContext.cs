using System.Text.Json;
using Logistics.Domain.Entities;
using Logistics.Infrastructure.Persistence.Extensions;
using Logistics.Infrastructure.Persistence.Helpers;
using Logistics.Infrastructure.Persistence.Interceptors;
using Logistics.Infrastructure.Persistence.Options;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.Persistence.Data;

public class TenantDbContext : DbContext
{
    private readonly AuditableEntitySaveChangesInterceptor? auditableEntity;

    // Default fallback connection string for local development and testing
    private readonly string defaultConnectionString;

    private readonly DispatchDomainEventsInterceptor? dispatchDomain;
    private readonly ILogger<TenantDbContext>? logger;

    public TenantDbContext(
        TenantDbContextOptions? tenantDbContextOptions = null,
        DispatchDomainEventsInterceptor? dispatchDomain = null,
        AuditableEntitySaveChangesInterceptor? auditableEntity = null,
        ILogger<TenantDbContext>? logger = null)
    {
        this.dispatchDomain = dispatchDomain;
        this.auditableEntity = auditableEntity;
        this.logger = logger;

        defaultConnectionString = tenantDbContextOptions?.ConnectionString
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
        logger?.LogInformation("Switched tenant database to '{TenantName}'.", tenant.Name);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (dispatchDomain is not null)
        {
            options.AddInterceptors(dispatchDomain);
        }

        if (auditableEntity is not null)
        {
            options.AddInterceptors(auditableEntity);
        }

        if (!options.IsConfigured)
        {
            DbContextHelpers.ConfigurePostgreSql(defaultConnectionString, options);
            logger?.LogInformation("Configured tenant database with default connection string.");
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Scan and apply all configurations from the /Data/Configurations folder
        // for entities implementing ITenantEntity
        builder.ApplyTenantConfigurationsFromAssembly<TenantDbContext>();

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
