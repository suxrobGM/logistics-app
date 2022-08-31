using Logistics.EntityFramework.Helpers;
using Logistics.Domain.Services;

namespace Logistics.EntityFramework.Data;

public class TenantDbContext : DbContext
{
    private readonly ITenantService? _tenantService;
    private readonly string _connectionString;

    public TenantDbContext(string connectionString)
    {
        _connectionString = connectionString;
        _tenantService = null;
    }

    public TenantDbContext(
        DbContextOptions<TenantDbContext> options, 
        ITenantService tenantService = null!)
        : base(options)
    {
        _tenantService = tenantService;
        _connectionString = string.Empty;
    }

    public Tenant? CurrentTenant => _tenantService?.GetTenant();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (options.IsConfigured)
            return;

        var connectionString = !string.IsNullOrEmpty(_connectionString)
            ? _connectionString
            : _tenantService?.GetConnectionString() ??
              throw new InvalidOperationException("Connection string is a null in the TenantDbContext");
        
        DbContextHelpers.ConfigureMySql(connectionString, options);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<TenantRole>(entity =>
        {
            entity.ToTable("roles");
        });

        builder.Entity<Employee>(entity =>
        {
            entity.ToTable("employees");
            entity.HasMany(i => i.Roles)
                .WithMany(i => i.Employees)
                .UsingEntity("employee_roles");
        });

        builder.Entity<Truck>(entity =>
        {
            entity.ToTable("trucks");

            entity.HasOne(m => m.Driver)
                .WithOne()
                .HasForeignKey<Truck>(m => m.DriverId);

            entity.HasMany(m => m.Loads)
                .WithOne(m => m.AssignedTruck)
                .HasForeignKey(m => m.AssignedTruckId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Load>(entity =>
        {
            entity.ToTable("loads");
            //entity.OwnsOne(m => m.SourceAddress);
            //entity.OwnsOne(m => m.DestinationAddress);
            entity.OwnsOne(m => m.Status);
            entity.HasIndex(m => m.RefId).IsUnique();

            entity.HasOne(m => m.AssignedDispatcher)
                .WithMany(m => m.DispatchedLoads)
                .HasForeignKey(m => m.AssignedDispatcherId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(m => m.AssignedDriver)
                .WithMany(m => m.DeliveredLoads)
                .HasForeignKey(m => m.AssignedDriverId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
