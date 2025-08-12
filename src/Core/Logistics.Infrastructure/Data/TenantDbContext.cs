using System.Text.Json;

using Logistics.Domain.Services;
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
    private readonly string _connectionString;
    private readonly DispatchDomainEventsInterceptor? _dispatchDomain;
    private readonly ILogger<TenantDbContext>? _logger;
    private readonly ITenantService? _tenantService;

    public TenantDbContext(
        TenantDbContextOptions? tenantDbContextOptions = null,
        ITenantService? tenantService = null,
        DispatchDomainEventsInterceptor? dispatchDomain = null,
        AuditableEntitySaveChangesInterceptor? auditableEntity = null,
        ILogger<TenantDbContext>? logger = null)
    {
        _dispatchDomain = dispatchDomain;
        _auditableEntity = auditableEntity;
        _connectionString = tenantDbContextOptions?.ConnectionString ?? ConnectionStrings.LocalDefaultTenant;
        _tenantService = tenantService;
        _logger = logger;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (_dispatchDomain is not null) options.AddInterceptors(_dispatchDomain);
        if (_auditableEntity is not null) options.AddInterceptors(_auditableEntity);

        if (!options.IsConfigured)
        {
            var tenantConnectionString = _connectionString;
            string? tenantName = null;

            // Configure the connection string based on the tenant data from the master database
            if (_tenantService is not null)
            {
                tenantConnectionString = _tenantService.GetTenant().ConnectionString;
                tenantName = _tenantService.GetTenant().Name;
            }

            DbContextHelpers.ConfigurePostgreSql(tenantConnectionString, options);
            _logger?.LogInformation(
                "Configured tenant database for '{TenantName}' with connection string: {ConnectionString}", tenantName,
                tenantConnectionString);
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
