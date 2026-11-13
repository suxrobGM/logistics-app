using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Master
{
    /// <inheritdoc />
    public partial class DropRedundantIndexAndNormalizeConstraintNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_ifta_tax_rates_year_quarter",
                table: "ifta_tax_rates");

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
            migrationBuilder.CreateIndex(
                name: "ix_ifta_tax_rates_year_quarter",
                table: "ifta_tax_rates",
                columns: new[] { "year", "quarter" });
        }
    }
}
