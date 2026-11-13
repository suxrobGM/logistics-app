using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class StoreOdometerAndMileageInMeters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Stored miles become meters in place; odometers were whole miles.
            foreach (var table in OdometerTables)
            {
                migrationBuilder.Sql($"""
                    ALTER TABLE {table} RENAME COLUMN odometer_reading TO odometer_meters;
                    ALTER TABLE {table} ALTER COLUMN odometer_meters TYPE bigint USING round(odometer_meters * 1609.344);
                    """);
            }

            migrationBuilder.Sql("""
                ALTER TABLE truck_jurisdiction_mileage RENAME COLUMN miles TO meters;
                ALTER TABLE truck_jurisdiction_mileage ALTER COLUMN meters TYPE numeric(14,2) USING round(meters * 1609.344, 2);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var table in OdometerTables)
            {
                migrationBuilder.Sql($"""
                    ALTER TABLE {table} ALTER COLUMN odometer_meters TYPE integer USING round(odometer_meters / 1609.344);
                    ALTER TABLE {table} RENAME COLUMN odometer_meters TO odometer_reading;
                    """);
            }

            migrationBuilder.Sql("""
                ALTER TABLE truck_jurisdiction_mileage ALTER COLUMN meters TYPE numeric(12,2) USING round(meters / 1609.344, 2);
                ALTER TABLE truck_jurisdiction_mileage RENAME COLUMN meters TO miles;
                """);
        }

        private static readonly string[] OdometerTables =
            ["truck_location_history", "maintenance_records", "expenses", "dvir_reports"];
    }
}
