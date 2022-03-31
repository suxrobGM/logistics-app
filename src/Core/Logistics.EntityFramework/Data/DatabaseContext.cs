using Microsoft.EntityFrameworkCore.Design;
using Logistics.EntityFramework.Helpers;

namespace Logistics.EntityFramework.Data;

public class DatabaseContext : DbContext
{
    private string connectionString;

    public DatabaseContext(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public DatabaseContext(DbContextOptions options)
        : base(options)
    {
        connectionString = ConnectionStrings.Local;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
        {
            DbContextHelpers.ConfigureMySql(connectionString, options);
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>(entity =>
        {
            entity.ToTable("users");
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
            entity.HasOne(m => m.AssignedDispatcher)
                .WithOne()
                .HasForeignKey<Cargo>(m => m.AssignedDispatcherId);
        });
    }
}

public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(string[] args)
    {
        return new DatabaseContext(ConnectionStrings.Local);
    }
}