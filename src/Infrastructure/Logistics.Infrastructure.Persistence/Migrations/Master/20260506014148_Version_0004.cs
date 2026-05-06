using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Master
{
    /// <inheritdoc />
    public partial class Version_0004 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "company_registration_number",
                table: "tenants",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "eori_number",
                table: "tenants",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "mc_number",
                table: "tenants",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tax_residency_country",
                table: "tenants",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "vat_number",
                table: "tenants",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "subtotal_amount",
                table: "invoices",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "subtotal_currency",
                table: "invoices",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "tax_behavior",
                table: "invoices",
                type: "text",
                nullable: false,
                defaultValue: "exclusive");

            migrationBuilder.AddColumn<string>(
                name: "tax_breakdown_json",
                table: "invoices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "tax_total_amount",
                table: "invoices",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "tax_total_currency",
                table: "invoices",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "tenant_tax_rates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rate_percent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    tax_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    effective_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    effective_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    jurisdiction_country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    jurisdiction_region = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenant_tax_rates", x => x.id);
                    table.ForeignKey(
                        name: "fk_tenant_tax_rates_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tenant_tax_rates_tenant_id_effective_from",
                table: "tenant_tax_rates",
                columns: new[] { "tenant_id", "effective_from" });

            // Backfill: existing invoices were created before Subtotal/TaxTotal existed.
            // Mirror Subtotal from Total and zero out the tax columns; TaxBehavior's column default
            // ('exclusive') already covers the new column on existing rows.
            migrationBuilder.Sql(
                """
                UPDATE invoices
                SET subtotal_amount   = total_amount,
                    subtotal_currency = total_currency,
                    tax_total_amount  = 0,
                    tax_total_currency = total_currency
                WHERE subtotal_currency = '' OR subtotal_currency IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tenant_tax_rates");

            migrationBuilder.DropColumn(
                name: "company_registration_number",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "eori_number",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "mc_number",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "tax_residency_country",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "vat_number",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "subtotal_amount",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "subtotal_currency",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "tax_behavior",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "tax_breakdown_json",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "tax_total_amount",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "tax_total_currency",
                table: "invoices");
        }
    }
}
