using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class Version_0005 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AddColumn<decimal>(
                name: "tax_amount",
                table: "invoice_line_items",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "tax_code",
                table: "invoice_line_items",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "tax_rate_percent",
                table: "invoice_line_items",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "is_vat_exempt",
                table: "customers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "tax_id",
                table: "customers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

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

            migrationBuilder.DropColumn(
                name: "tax_amount",
                table: "invoice_line_items");

            migrationBuilder.DropColumn(
                name: "tax_code",
                table: "invoice_line_items");

            migrationBuilder.DropColumn(
                name: "tax_rate_percent",
                table: "invoice_line_items");

            migrationBuilder.DropColumn(
                name: "is_vat_exempt",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "tax_id",
                table: "customers");
        }
    }
}
