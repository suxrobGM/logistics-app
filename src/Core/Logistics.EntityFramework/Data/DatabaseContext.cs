using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Logistics.Domain.Entities;
using Logistics.EntityFramework.Helpers;

namespace Logistics.EntityFramework.Data;

public class DatabaseContext : IdentityDbContext<User, UserRole, string>
{
    private string connectionString;

    public DatabaseContext(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public DatabaseContext(DbContextOptions options)
        : base(options)
    {
        connectionString = DefualtConnection.ConnectionString;
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

        builder.Entity<Truck>(entity =>
        {
            entity.HasOne(m => m.Driver)
            .WithOne()
            .HasForeignKey<Truck>(m => m.DriverId);

            entity.HasMany(m => m.Cargoes)
            .WithOne(m => m.AssignedTruck)
            .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Cargo>(entity =>
        {
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
        return new DatabaseContext(DefualtConnection.ConnectionString);
    }
}