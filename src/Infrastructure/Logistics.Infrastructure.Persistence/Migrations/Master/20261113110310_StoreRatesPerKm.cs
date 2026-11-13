using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Master
{
    /// <inheritdoc />
    public partial class StoreRatesPerKm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Per-mile values become per km in place (1 mi = 1.609344 km).
            migrationBuilder.Sql("""
                ALTER TABLE tenants RENAME COLUMN settings_cost_per_mile TO settings_cost_per_km;
                ALTER TABLE tenants ALTER COLUMN settings_cost_per_km TYPE numeric(18,4) USING round(settings_cost_per_km / 1.609344, 4);
                ALTER TABLE tenants RENAME COLUMN settings_default_rate_floor_per_mile TO settings_default_rate_floor_per_km;
                ALTER TABLE tenants ALTER COLUMN settings_default_rate_floor_per_km TYPE numeric(18,4) USING round(settings_default_rate_floor_per_km / 1.609344, 4);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE tenants ALTER COLUMN settings_cost_per_km TYPE numeric(18,2) USING round(settings_cost_per_km * 1.609344, 2);
                ALTER TABLE tenants RENAME COLUMN settings_cost_per_km TO settings_cost_per_mile;
                ALTER TABLE tenants ALTER COLUMN settings_default_rate_floor_per_km TYPE numeric(18,2) USING round(settings_default_rate_floor_per_km * 1.609344, 2);
                ALTER TABLE tenants RENAME COLUMN settings_default_rate_floor_per_km TO settings_default_rate_floor_per_mile;
                """);
        }
    }
}
