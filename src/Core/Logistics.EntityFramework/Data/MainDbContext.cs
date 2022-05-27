using Logistics.EntityFramework.Helpers;
using Microsoft.EntityFrameworkCore.Design;

namespace Logistics.EntityFramework.Data;

public class MainDbContext : DbContext
{
    private readonly string _connectionString;

    public MainDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public MainDbContext(DbContextOptions options)
        : base(options)
    {
        _connectionString = ConnectionStrings.LocalDefaultTenant;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
        {
            DbContextHelpers.ConfigureMySql(_connectionString, options);
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Tenant>(entity =>
        {
            entity.ToTable("tenants");
        });
    }
}
