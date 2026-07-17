using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Master
{
    /// <inheritdoc />
    public partial class AddIftaTaxRates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ifta_tax_rates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    quarter = table.Column<int>(type: "integer", nullable: false),
                    rate_per_gallon = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: false),
                    surcharge_rate_per_gallon = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: true),
                    jurisdiction_country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    jurisdiction_region = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ifta_tax_rates", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_ifta_tax_rates_year_quarter",
                table: "ifta_tax_rates",
                columns: new[] { "year", "quarter" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ifta_tax_rates");
        }
    }
}
