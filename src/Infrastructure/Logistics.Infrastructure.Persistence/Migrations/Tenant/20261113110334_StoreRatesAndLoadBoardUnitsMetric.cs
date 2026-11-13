using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class StoreRatesAndLoadBoardUnitsMetric : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rates per mile become per km (1 mi = 1.609344 km); load board miles, pounds and feet become
            // meters, kilograms and meters. Values convert in place.
            migrationBuilder.Sql("""
                ALTER TABLE lane_rate_floors RENAME COLUMN min_rate_per_mile TO min_rate_per_km;
                ALTER TABLE lane_rate_floors ALTER COLUMN min_rate_per_km TYPE numeric(18,4) USING round(min_rate_per_km / 1.609344, 4);
                """);

            migrationBuilder.Sql("""
                ALTER TABLE rate_negotiations RENAME COLUMN floor_rate_per_mile TO floor_rate_per_km;
                ALTER TABLE rate_negotiations ALTER COLUMN floor_rate_per_km TYPE numeric(18,4) USING round(floor_rate_per_km / 1.609344, 4);
                """);

            migrationBuilder.Sql("""
                ALTER TABLE negotiation_messages RENAME COLUMN proposed_rate_per_mile TO proposed_rate_per_km;
                ALTER TABLE negotiation_messages ALTER COLUMN proposed_rate_per_km TYPE numeric(18,4) USING round(proposed_rate_per_km / 1.609344, 4);
                """);

            migrationBuilder.Sql("""
                ALTER TABLE load_board_listings RENAME COLUMN rate_per_mile TO rate_per_km;
                ALTER TABLE load_board_listings ALTER COLUMN rate_per_km TYPE numeric(18,4) USING round(rate_per_km / 1.609344, 4);
                UPDATE load_board_listings SET distance = distance * 1609.344;
                ALTER TABLE load_board_listings RENAME COLUMN weight TO weight_kg;
                ALTER TABLE load_board_listings ALTER COLUMN weight_kg TYPE numeric(18,2) USING round(weight_kg * 0.45359237, 2);
                ALTER TABLE load_board_listings RENAME COLUMN length TO length_meters;
                ALTER TABLE load_board_listings ALTER COLUMN length_meters TYPE numeric(18,2) USING round(length_meters * 0.3048, 2);
                """);

            migrationBuilder.Sql("""
                ALTER TABLE posted_trucks RENAME COLUMN destination_radius TO destination_radius_meters;
                UPDATE posted_trucks SET destination_radius_meters = round(destination_radius_meters * 1609.344);
                ALTER TABLE posted_trucks RENAME COLUMN max_weight TO max_weight_kg;
                ALTER TABLE posted_trucks ALTER COLUMN max_weight_kg TYPE numeric(18,2) USING round(max_weight_kg * 0.45359237, 2);
                ALTER TABLE posted_trucks RENAME COLUMN max_length TO max_length_meters;
                ALTER TABLE posted_trucks ALTER COLUMN max_length_meters TYPE numeric(18,2) USING round(max_length_meters * 0.3048, 2);
                """);

            migrationBuilder.Sql("""
                ALTER TABLE employees ALTER COLUMN salary TYPE numeric(18,4);
                UPDATE employees SET salary = round(salary / 1.609344, 4) WHERE salary_type = 'rate_per_distance';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE employees SET salary = salary * 1.609344 WHERE salary_type = 'rate_per_distance';
                ALTER TABLE employees ALTER COLUMN salary TYPE numeric(18,2) USING round(salary, 2);
                """);

            migrationBuilder.Sql("""
                ALTER TABLE posted_trucks ALTER COLUMN max_length_meters TYPE integer USING round(max_length_meters / 0.3048);
                ALTER TABLE posted_trucks RENAME COLUMN max_length_meters TO max_length;
                ALTER TABLE posted_trucks ALTER COLUMN max_weight_kg TYPE integer USING round(max_weight_kg / 0.45359237);
                ALTER TABLE posted_trucks RENAME COLUMN max_weight_kg TO max_weight;
                UPDATE posted_trucks SET destination_radius_meters = round(destination_radius_meters / 1609.344);
                ALTER TABLE posted_trucks RENAME COLUMN destination_radius_meters TO destination_radius;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE load_board_listings ALTER COLUMN length_meters TYPE integer USING round(length_meters / 0.3048);
                ALTER TABLE load_board_listings RENAME COLUMN length_meters TO length;
                ALTER TABLE load_board_listings ALTER COLUMN weight_kg TYPE integer USING round(weight_kg / 0.45359237);
                ALTER TABLE load_board_listings RENAME COLUMN weight_kg TO weight;
                UPDATE load_board_listings SET distance = distance / 1609.344;
                ALTER TABLE load_board_listings ALTER COLUMN rate_per_km TYPE numeric(18,2) USING round(rate_per_km * 1.609344, 2);
                ALTER TABLE load_board_listings RENAME COLUMN rate_per_km TO rate_per_mile;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE negotiation_messages ALTER COLUMN proposed_rate_per_km TYPE numeric(18,2) USING round(proposed_rate_per_km * 1.609344, 2);
                ALTER TABLE negotiation_messages RENAME COLUMN proposed_rate_per_km TO proposed_rate_per_mile;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE rate_negotiations ALTER COLUMN floor_rate_per_km TYPE numeric(18,2) USING round(floor_rate_per_km * 1.609344, 2);
                ALTER TABLE rate_negotiations RENAME COLUMN floor_rate_per_km TO floor_rate_per_mile;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE lane_rate_floors ALTER COLUMN min_rate_per_km TYPE numeric(18,2) USING round(min_rate_per_km * 1.609344, 2);
                ALTER TABLE lane_rate_floors RENAME COLUMN min_rate_per_km TO min_rate_per_mile;
                """);
        }
    }
}
