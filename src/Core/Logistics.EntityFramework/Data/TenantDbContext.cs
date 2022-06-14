using Logistics.EntityFramework.Helpers;
using Logistics.Domain.Services;

namespace Logistics.EntityFramework.Data;

public class TenantDbContext : DbContext
{
    private readonly ITenantService _tenantService;
    private readonly string _connectionString;

    public TenantDbContext(string connectionString)
    {
        _connectionString = connectionString;
        _tenantService = null!;
    }

    public TenantDbContext(
        DbContextOptions<TenantDbContext> options, 
        ITenantService tenantService = null!)
        : base(options)
    {
        _tenantService = tenantService;
        _connectionString = string.Empty;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (options.IsConfigured)
            return;      

        if (!string.IsNullOrEmpty(_connectionString))
        {
            DbContextHelpers.ConfigureMySql(_connectionString, options);
        }
        else if (_tenantService != null)
        {
            DbContextHelpers.ConfigureMySql(_tenantService.GetConnectionString(), options);
        }
        else
        {
            throw new InvalidOperationException("Could not configure the tenant database");
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Employee>(entity =>
        {
            entity.ToTable("employees");
            entity.HasMany(m => m.DispatcherCargoes)
                .WithOne(m => m.AssignedDispatcher)
                .HasForeignKey(m => m.AssignedDispatcherId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.OwnsOne(m => m.Role);
        });

        builder.Entity<Truck>(entity =>
        {
            entity.ToTable("trucks");

            entity.HasOne(m => m.Driver)
                .WithOne()
                .HasForeignKey<Truck>(m => m.DriverId);

            entity.HasMany(m => m.Cargoes)
                .WithOne(m => m.AssignedTruck)
                .HasForeignKey(m => m.AssignedTruckId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Cargo>(entity =>
        {
            entity.ToTable("cargoes");
            entity.OwnsOne(m => m.Status);
        });
    }
}
