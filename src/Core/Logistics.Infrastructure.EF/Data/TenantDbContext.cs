using Logistics.Domain.Entities;
using Logistics.Domain.Services;
using Logistics.Infrastructure.EF.Data.Configurations;
using Logistics.Infrastructure.EF.Helpers;
using Logistics.Infrastructure.EF.Interceptors;
using Logistics.Infrastructure.EF.Options;
using Logistics.Shared.Consts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.EF.Data;

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
        
        builder.ApplyConfiguration(new AuditableEntityConfiguration());
        builder.Entity<TenantRoleClaim>().ToTable("RoleClaims");
        builder.Entity<Notification>().ToTable("Notifications");
        builder.Entity<Customer>().ToTable("Customers");
        
        builder.Entity<Payment>(cfg =>
        {
            cfg.ToTable("Payments");
        });
        
        // ── Invoice (TPH)
        builder.Entity<Invoice>(cfg =>
        {
            cfg.ToTable("Invoices")
                .HasDiscriminator(i => i.Type)
                .HasValue<LoadInvoice>(InvoiceType.Load)
                .HasValue<SubscriptionInvoice>(InvoiceType.Subscription)
                .HasValue<PayrollInvoice>(InvoiceType.Payroll);
            
            cfg.Property(i => i.Number)
                .UseIdentityAlwaysColumn()
                .IsRequired();

            cfg.HasIndex(i => i.Number)
                .IsUnique();

            // Payments (1-many)
            // cfg.HasMany<Payment>()
            //     .WithOne(p => p.Invoice)
            //     .HasForeignKey(p => p.InvoiceId);
        });

        // Fine-tune derived types for LoadInvoice, SubscriptionInvoice, and PayrollInvoice
        builder.Entity<LoadInvoice>()
            .HasOne(i => i.Load)
            .WithMany(l => l.Invoices)
            .HasForeignKey(i => i.LoadId);

        builder.Entity<SubscriptionInvoice>()
            .HasOne(i => i.Subscription)
            .WithMany(s => s.Invoices)
            .HasForeignKey(i => i.SubscriptionId);

        builder.Entity<PayrollInvoice>()
            .HasOne(i => i.Employee)
            .WithMany(e => e.PayrollInvoices)
            .HasForeignKey(i => i.EmployeeId);

        builder.Entity<TenantRole>(cfg =>
        {
            cfg.ToTable("Roles");
            cfg.HasMany(i => i.Claims)
                .WithOne(i => i.Role)
                .HasForeignKey(i => i.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Employee>(cfg =>
        {
            cfg.ToTable("Employees");
            cfg.Property(i => i.Salary).HasPrecision(18, 2);
            
            cfg.HasMany(i => i.Roles)
                .WithMany(i => i.Employees)
                .UsingEntity<EmployeeTenantRole>(
                    l => l.HasOne<TenantRole>(i => i.Role).WithMany(i => i.EmployeeRoles),
                    r => r.HasOne<Employee>(i => i.Employee).WithMany(i => i.EmployeeRoles),
                    c => c.ToTable("EmployeeRoles"));
        });

        builder.Entity<Truck>(cfg =>
        {
            cfg.ToTable("Trucks");

            cfg.HasMany(i => i.Drivers)
                .WithOne(i => i.Truck)
                .HasForeignKey(i => i.TruckId)
                .OnDelete(DeleteBehavior.SetNull);

            cfg.HasMany(i => i.Loads)
                .WithOne(i => i.AssignedTruck)
                .HasForeignKey(i => i.AssignedTruckId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Load>(cfg =>
        {
            cfg.ToTable("Loads");
            //entity.OwnsOne(m => m.SourceAddress);
            //entity.OwnsOne(m => m.DestinationAddress);
            cfg.Property(i => i.DeliveryCost).HasPrecision(18, 2);
            
            cfg.Property(i => i.Number)
                .UseIdentityAlwaysColumn()
                .IsRequired();

            cfg.HasIndex(i => i.Number)
                .IsUnique();

            cfg.HasOne(i => i.AssignedDispatcher)
                .WithMany(i => i.DispatchedLoads)
                .HasForeignKey(i => i.AssignedDispatcherId);
                //.OnDelete(DeleteBehavior.SetNull);
            
            // entity.HasOne(m => m.AssignedDriver)
            //     .WithMany(m => m.DeliveredLoads)
            //     .HasForeignKey(m => m.AssignedDriverId);
                //.OnDelete(DeleteBehavior.SetNull);
        });
        
        builder.Entity<PaymentMethod>(cfg =>
        {
            cfg.ToTable("PaymentMethods")
                .HasDiscriminator(pm => pm.Type)
                .HasValue<CardPaymentMethod>(PaymentMethodType.Card)
                .HasValue<UsBankAccountPaymentMethod>(PaymentMethodType.UsBankAccount)
                .HasValue<BankAccountPaymentMethod>(PaymentMethodType.InternationalBankAccount);

            cfg.HasIndex(i => i.StripePaymentMethodId);
        });
    }
}
