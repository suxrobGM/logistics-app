using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddAccidentFatalitiesTowAndHazmat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "fatalities_reported",
                table: "accident_reports",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "hazmat_description",
                table: "accident_reports",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "hazmat_involved",
                table: "accident_reports",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "number_of_fatalities",
                table: "accident_reports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tow_company",
                table: "accident_reports",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "vehicle_towed",
                table: "accident_reports",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fatalities_reported",
                table: "accident_reports");

            migrationBuilder.DropColumn(
                name: "hazmat_description",
                table: "accident_reports");

            migrationBuilder.DropColumn(
                name: "hazmat_involved",
                table: "accident_reports");

            migrationBuilder.DropColumn(
                name: "number_of_fatalities",
                table: "accident_reports");

            migrationBuilder.DropColumn(
                name: "tow_company",
                table: "accident_reports");

            migrationBuilder.DropColumn(
                name: "vehicle_towed",
                table: "accident_reports");
        }
    }
}
