using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Master
{
    /// <inheritdoc />
    public partial class AddTenantDetentionTerms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "settings_detention_free_minutes",
                table: "tenants",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "settings_detention_rate_per_hour",
                table: "tenants",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "factoring_status",
                table: "invoices",
                type: "text",
                nullable: true,
                defaultValue: "not_factored");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "settings_detention_free_minutes",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "settings_detention_rate_per_hour",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "factoring_status",
                table: "invoices");
        }
    }
}
