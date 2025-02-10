using Logistics.Domain.Entities;
using Logistics.Domain.Services;
using Logistics.Infrastructure.EF.Helpers;
using Logistics.Infrastructure.EF.Interceptors;
using Logistics.Infrastructure.EF.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.EF.Data;

public class TenantDbContext : DbContext
{
    private readonly DispatchDomainEventsInterceptor? _dispatchDomainEventsInterceptor;
    private readonly string _connectionString;
    private readonly ILogger<TenantDbContext>? _logger;

    public TenantDbContext(
        TenantDbContextOptions? tenantDbContextOptions = null, 
        ITenantService? tenantService = null,
        DispatchDomainEventsInterceptor? dispatchDomainEventsInterceptor = null,
        ILogger<TenantDbContext>? logger = null)
    {
        _dispatchDomainEventsInterceptor = dispatchDomainEventsInterceptor;
        _connectionString = tenantDbContextOptions?.ConnectionString ?? ConnectionStrings.LocalDefaultTenant;
        TenantService = tenantService;
        _logger = logger;
    }

    internal ITenantService? TenantService { get; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (_dispatchDomainEventsInterceptor is not null)
        {
            options.AddInterceptors(_dispatchDomainEventsInterceptor);
        }

        if (!options.IsConfigured)
        {
            var tenantConnectionString = _connectionString;
            string? tenantName = null;
            
            if (TenantService is not null)
            {
                tenantConnectionString = TenantService.GetTenant().ConnectionString;
                tenantName = TenantService.GetTenant().Name;
            }
            
            DbContextHelpers.ConfigureSqlServer(tenantConnectionString, options);
            _logger?.LogInformation("Configured tenant database for '{TenantName}' with connection string: {ConnectionString}", tenantName, tenantConnectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<TenantRoleClaim>().ToTable("RoleClaims");
        builder.Entity<Notification>().ToTable("Notifications");
        builder.Entity<Customer>().ToTable("Customers");
        
        builder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payments");
            entity.Property(i => i.Amount).HasPrecision(18, 2);
        });

        builder.Entity<Invoice>(entity =>
        {
            entity.ToTable("Invoices");

            entity.HasOne(i => i.Load)
                .WithOne(i => i.Invoice)
                .HasForeignKey<Load>(i => i.InvoiceId);
            
            entity.HasOne(i => i.Customer)
                .WithMany(i => i.Invoices)
                .HasForeignKey(i => i.CustomerId);

            entity.HasOne(i => i.Payment)
                .WithOne()
                .HasForeignKey<Invoice>(i => i.PaymentId);
        });

        builder.Entity<Payroll>(entity =>
        {
            entity.ToTable("Payrolls");

            entity.HasOne(i => i.Payment)
                .WithOne()
                .HasForeignKey<Payroll>(i => i.PaymentId);
            
            entity.HasOne(i => i.Employee)
                .WithMany(i => i.PayrollPayments)
                .HasForeignKey(i => i.EmployeeId);
        });

        builder.Entity<TenantRole>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasMany(i => i.Claims)
                .WithOne(i => i.Role)
                .HasForeignKey(i => i.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Employee>(entity =>
        {
            entity.ToTable("Employees");
            entity.Property(i => i.Salary).HasPrecision(18, 2);
            
            entity.HasMany(i => i.Roles)
                .WithMany(i => i.Employees)
                .UsingEntity<EmployeeTenantRole>(
                    l => l.HasOne<TenantRole>(i => i.Role).WithMany(i => i.EmployeeRoles),
                    r => r.HasOne<Employee>(i => i.Employee).WithMany(i => i.EmployeeRoles),
                    c => c.ToTable("EmployeeRoles"));
        });

        builder.Entity<Truck>(entity =>
        {
            entity.ToTable("Trucks");

            entity.HasMany(i => i.Drivers)
                .WithOne(i => i.Truck)
                .HasForeignKey(i => i.TruckId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(i => i.Loads)
                .WithOne(i => i.AssignedTruck)
                .HasForeignKey(i => i.AssignedTruckId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Load>(entity =>
        {
            entity.ToTable("Loads");
            //entity.OwnsOne(m => m.SourceAddress);
            //entity.OwnsOne(m => m.DestinationAddress);
            entity.Property(i => i.DeliveryCost).HasPrecision(18, 2);
            entity.HasIndex(i => i.RefId).IsUnique();

            entity.HasOne(i => i.AssignedDispatcher)
                .WithMany(i => i.DispatchedLoads)
                .HasForeignKey(i => i.AssignedDispatcherId);
                //.OnDelete(DeleteBehavior.SetNull);
            
            // entity.HasOne(m => m.AssignedDriver)
            //     .WithMany(m => m.DeliveredLoads)
            //     .HasForeignKey(m => m.AssignedDriverId);
                //.OnDelete(DeleteBehavior.SetNull);
        });
    }
}
