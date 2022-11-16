using Logistics.Infrastructure.EF.Helpers;
using Logistics.Infrastructure.EF.Options;

namespace Logistics.Infrastructure.EF.Data;

public class TenantDbContext : DbContext
{
    private readonly ITenantService? _tenantService;
    private readonly string _connectionString;

    public TenantDbContext(
        TenantDbContextOptions options, 
        ITenantService? tenantService)
    {
        _connectionString = options.ConnectionString ?? ConnectionStrings.LocalDefaultTenant;
        _tenantService = tenantService;
    }

    public Tenant? CurrentTenant => _tenantService?.GetTenant();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (options.IsConfigured)
            return;

        var connectionString = _tenantService?.GetConnectionString() ?? _connectionString;
        DbContextHelpers.ConfigureMySql(connectionString, options);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<TenantRoleClaim>().ToTable("role_claims");

        builder.Entity<TenantRole>(entity =>
        {
            entity.ToTable("roles");
            entity.HasMany(m => m.Claims)
                .WithOne(m => m.Role)
                .HasForeignKey(m => m.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
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
