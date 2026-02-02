using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Master
{
    /// <inheritdoc />
    public partial class Version_0002 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Settings_Currency",
                table: "Tenants",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Settings_DateFormat",
                table: "Tenants",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Settings_DistanceUnit",
                table: "Tenants",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Settings_Timezone",
                table: "Tenants",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Settings_WeightUnit",
                table: "Tenants",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Settings_Currency",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "Settings_DateFormat",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "Settings_DistanceUnit",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "Settings_Timezone",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "Settings_WeightUnit",
                table: "Tenants");
        }
    }
}
