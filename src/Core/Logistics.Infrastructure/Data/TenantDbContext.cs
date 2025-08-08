using Logistics.Domain.Entities;
using Logistics.Domain.Services;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Logistics.Infrastructure.Data.Configurations;
using Logistics.Infrastructure.Helpers;
using Logistics.Infrastructure.Interceptors;
using Logistics.Infrastructure.Options;

namespace Logistics.Infrastructure.Data;

public class TenantDbContext : DbContext
{
    private readonly DispatchDomainEventsInterceptor? _dispatchDomain;
    private readonly AuditableEntitySaveChangesInterceptor? _auditableEntity;
    private readonly string _connectionString;
    private readonly ILogger<TenantDbContext>? _logger;

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
        TenantService = tenantService;
        _logger = logger;
    }

    internal ITenantService? TenantService { get; }

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
            var tenantConnectionString = _connectionString;
            string? tenantName = null;
            
            // Configure the connection string based on the tenant data from the master database
            if (TenantService is not null)
            {
                tenantConnectionString = TenantService.GetTenant().ConnectionString;
                tenantName = TenantService.GetTenant().Name;
            }

            DbContextHelpers.ConfigurePostgreSql(tenantConnectionString, options);
            _logger?.LogInformation("Configured tenant database for '{TenantName}' with connection string: {ConnectionString}", tenantName, tenantConnectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        //builder.ApplyConfiguration(new AuditableEntityConfiguration());
        builder.ApplyConfiguration(new InvoiceEntityConfiguration());
        builder.ApplyConfiguration(new InvoiceEntityConfiguration.LoadInvoiceEntityConfiguration());
        builder.ApplyConfiguration(new InvoiceEntityConfiguration.SubscriptionInvoiceEntityConfiguration());
        builder.ApplyConfiguration(new InvoiceEntityConfiguration.PayrollInvoiceEntityConfiguration());
        builder.ApplyConfiguration(new LoadEntityConfiguration());
        builder.ApplyConfiguration(new LoadDocumentEntityConfiguration());
        builder.ApplyConfiguration(new PaymentEntityConfiguration());
        builder.ApplyConfiguration(new PaymentMethodEntityConfiguration());
        builder.ApplyConfiguration(new EmployeeEntityConfiguration());
        builder.ApplyConfiguration(new TenantRoleEntityConfiguration());
        builder.ApplyConfiguration(new TruckEntityConfiguration());
        builder.ApplyConfiguration(new TripEntityConfiguration());
        builder.ApplyConfiguration(new TripStopEntityConfiguration());

        builder.Entity<TenantRoleClaim>().ToTable("RoleClaims");
        builder.Entity<Notification>().ToTable("Notifications");
        builder.Entity<Customer>().ToTable("Customers");
        
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
