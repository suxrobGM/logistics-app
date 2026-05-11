using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Master
{
    /// <inheritdoc />
    public partial class Version_0009 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "settings_temperature_unit",
                table: "tenants",
                type: "text",
                nullable: false,
                defaultValue: "Fahrenheit");

            migrationBuilder.AddColumn<string>(
                name: "settings_volume_unit",
                table: "tenants",
                type: "text",
                nullable: false,
                defaultValue: "Gallons");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "settings_temperature_unit",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "settings_volume_unit",
                table: "tenants");
        }
    }
}
