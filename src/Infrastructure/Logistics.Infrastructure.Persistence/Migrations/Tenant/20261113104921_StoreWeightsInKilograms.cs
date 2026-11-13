using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class StoreWeightsInKilograms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Container weights were already entered in kilograms. Load weights followed the tenant
            // unit but no tenant has recorded one, so a plain cast keeps whatever is there.
            migrationBuilder.RenameColumn(
                name: "gross_weight",
                table: "containers",
                newName: "gross_weight_kg");

            migrationBuilder.Sql("""
                ALTER TABLE loads RENAME COLUMN weight TO weight_kg;
                ALTER TABLE loads ALTER COLUMN weight_kg TYPE numeric(18,2) USING round(weight_kg::numeric, 2);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "gross_weight_kg",
                table: "containers",
                newName: "gross_weight");

            migrationBuilder.Sql("""
                ALTER TABLE loads ALTER COLUMN weight_kg TYPE double precision;
                ALTER TABLE loads RENAME COLUMN weight_kg TO weight;
                """);
        }
    }
}
