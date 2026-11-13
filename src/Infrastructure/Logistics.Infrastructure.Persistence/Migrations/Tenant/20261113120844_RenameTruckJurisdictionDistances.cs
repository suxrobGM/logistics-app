using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class RenameTruckJurisdictionDistances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE truck_jurisdiction_mileage RENAME TO truck_jurisdiction_distances;
                ALTER TABLE truck_jurisdiction_distances RENAME CONSTRAINT pk_truck_jurisdiction_mileage TO pk_truck_jurisdiction_distances;
                ALTER TABLE truck_jurisdiction_distances RENAME CONSTRAINT fk_truck_jurisdiction_mileage_trucks_truck_id TO fk_truck_jurisdiction_distances_trucks_truck_id;
                ALTER INDEX ix_truck_jurisdiction_mileage_date RENAME TO ix_truck_jurisdiction_distances_date;
                ALTER INDEX ix_truck_jurisdiction_mileage_truck_id_date RENAME TO ix_truck_jurisdiction_distances_truck_id_date;
                ALTER INDEX ux_truck_jurisdiction_mileage_truck_date_jurisdiction RENAME TO ux_truck_jurisdiction_distances_truck_date_jurisdiction;
                """);

            // Postgres names a NOT NULL constraint after the column it was created on and never
            // renames it, so renamed columns left stale names. Down leaves the new names.
            migrationBuilder.Sql("""
                DO $$
                DECLARE
                    r record;
                BEGIN
                    FOR r IN
                        SELECT c.conname, t.relname AS table_name,
                               left(t.relname || '_' || a.attname || '_not_null', 63) AS expected
                        FROM pg_constraint c
                        JOIN pg_class t ON t.oid = c.conrelid
                        JOIN pg_namespace n ON n.oid = t.relnamespace
                        JOIN pg_attribute a ON a.attrelid = t.oid AND a.attnum = c.conkey[1]
                        WHERE c.contype = 'n' AND n.nspname = current_schema()
                    LOOP
                        IF r.conname <> r.expected THEN
                            EXECUTE format('ALTER TABLE %I RENAME CONSTRAINT %I TO %I', r.table_name, r.conname, r.expected);
                        END IF;
                    END LOOP;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE truck_jurisdiction_distances RENAME TO truck_jurisdiction_mileage;
                ALTER TABLE truck_jurisdiction_mileage RENAME CONSTRAINT pk_truck_jurisdiction_distances TO pk_truck_jurisdiction_mileage;
                ALTER TABLE truck_jurisdiction_mileage RENAME CONSTRAINT fk_truck_jurisdiction_distances_trucks_truck_id TO fk_truck_jurisdiction_mileage_trucks_truck_id;
                ALTER INDEX ix_truck_jurisdiction_distances_date RENAME TO ix_truck_jurisdiction_mileage_date;
                ALTER INDEX ix_truck_jurisdiction_distances_truck_id_date RENAME TO ix_truck_jurisdiction_mileage_truck_id_date;
                ALTER INDEX ux_truck_jurisdiction_distances_truck_date_jurisdiction RENAME TO ux_truck_jurisdiction_mileage_truck_date_jurisdiction;
                """);
        }
    }
}
