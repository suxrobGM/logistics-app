using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class SingleTenantCurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "amount_currency",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "delivery_cost_currency",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "min_total_rate_currency",
                table: "lane_rate_floors");

            migrationBuilder.DropColumn(
                name: "subtotal_currency",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "tax_total_currency",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "total_currency",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "amount_currency",
                table: "invoice_line_items");

            migrationBuilder.DropColumn(
                name: "total_currency",
                table: "factoring_batches");

            migrationBuilder.DropColumn(
                name: "amount_currency",
                table: "factoring_batch_items");

            migrationBuilder.DropColumn(
                name: "amount_currency",
                table: "expenses");

            migrationBuilder.DropColumn(
                name: "salary_currency",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "amount_currency",
                table: "cash_advances");

            migrationBuilder.RenameColumn(
                name: "amount_amount",
                table: "payments",
                newName: "amount");

            migrationBuilder.RenameColumn(
                name: "delivery_cost_amount",
                table: "loads",
                newName: "delivery_cost");

            migrationBuilder.RenameColumn(
                name: "min_total_rate_amount",
                table: "lane_rate_floors",
                newName: "min_total_rate");

            migrationBuilder.RenameColumn(
                name: "total_amount",
                table: "invoices",
                newName: "total");

            migrationBuilder.RenameColumn(
                name: "tax_total_amount",
                table: "invoices",
                newName: "tax_total");

            migrationBuilder.RenameColumn(
                name: "subtotal_amount",
                table: "invoices",
                newName: "subtotal");

            migrationBuilder.RenameColumn(
                name: "amount_amount",
                table: "invoice_line_items",
                newName: "amount");

            migrationBuilder.RenameColumn(
                name: "amount_currency",
                table: "fuel_card_transactions",
                newName: "currency");

            migrationBuilder.RenameColumn(
                name: "amount_amount",
                table: "fuel_card_transactions",
                newName: "amount");

            migrationBuilder.RenameColumn(
                name: "total_amount",
                table: "factoring_batches",
                newName: "total");

            migrationBuilder.RenameColumn(
                name: "amount_amount",
                table: "factoring_batch_items",
                newName: "amount");

            migrationBuilder.RenameColumn(
                name: "amount_amount",
                table: "expenses",
                newName: "amount");

            migrationBuilder.RenameColumn(
                name: "salary_amount",
                table: "employees",
                newName: "salary");

            migrationBuilder.RenameColumn(
                name: "amount_amount",
                table: "cash_advances",
                newName: "amount");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "amount",
                table: "payments",
                newName: "amount_amount");

            migrationBuilder.RenameColumn(
                name: "delivery_cost",
                table: "loads",
                newName: "delivery_cost_amount");

            migrationBuilder.RenameColumn(
                name: "min_total_rate",
                table: "lane_rate_floors",
                newName: "min_total_rate_amount");

            migrationBuilder.RenameColumn(
                name: "total",
                table: "invoices",
                newName: "total_amount");

            migrationBuilder.RenameColumn(
                name: "tax_total",
                table: "invoices",
                newName: "tax_total_amount");

            migrationBuilder.RenameColumn(
                name: "subtotal",
                table: "invoices",
                newName: "subtotal_amount");

            migrationBuilder.RenameColumn(
                name: "amount",
                table: "invoice_line_items",
                newName: "amount_amount");

            migrationBuilder.RenameColumn(
                name: "currency",
                table: "fuel_card_transactions",
                newName: "amount_currency");

            migrationBuilder.RenameColumn(
                name: "amount",
                table: "fuel_card_transactions",
                newName: "amount_amount");

            migrationBuilder.RenameColumn(
                name: "total",
                table: "factoring_batches",
                newName: "total_amount");

            migrationBuilder.RenameColumn(
                name: "amount",
                table: "factoring_batch_items",
                newName: "amount_amount");

            migrationBuilder.RenameColumn(
                name: "amount",
                table: "expenses",
                newName: "amount_amount");

            migrationBuilder.RenameColumn(
                name: "salary",
                table: "employees",
                newName: "salary_amount");

            migrationBuilder.RenameColumn(
                name: "amount",
                table: "cash_advances",
                newName: "amount_amount");

            migrationBuilder.AddColumn<string>(
                name: "amount_currency",
                table: "payments",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "delivery_cost_currency",
                table: "loads",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "min_total_rate_currency",
                table: "lane_rate_floors",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "subtotal_currency",
                table: "invoices",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "tax_total_currency",
                table: "invoices",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "total_currency",
                table: "invoices",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "amount_currency",
                table: "invoice_line_items",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "total_currency",
                table: "factoring_batches",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "amount_currency",
                table: "factoring_batch_items",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "amount_currency",
                table: "expenses",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "salary_currency",
                table: "employees",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "amount_currency",
                table: "cash_advances",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");
        }
    }
}
