using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class Version_0009 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "quantity",
                table: "expenses",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "quantity_unit",
                table: "expenses",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "quantity",
                table: "expenses");

            migrationBuilder.DropColumn(
                name: "quantity_unit",
                table: "expenses");
        }
    }
}
