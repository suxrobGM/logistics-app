using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.EF.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class Version_0004 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Loads");

            migrationBuilder.AddColumn<bool>(
                name: "CanConfirmDelivery",
                table: "Loads",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanConfirmPickUp",
                table: "Loads",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanConfirmDelivery",
                table: "Loads");

            migrationBuilder.DropColumn(
                name: "CanConfirmPickUp",
                table: "Loads");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Loads",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
