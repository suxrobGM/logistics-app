using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddCustomerUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "customer_updates",
                table: "loads",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<bool>(
                name: "update_preferences_delivered",
                table: "customers",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "update_preferences_dispatched",
                table: "customers",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "update_preferences_late",
                table: "customers",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "update_preferences_picked_up",
                table: "customers",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "customer_updates",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "update_preferences_delivered",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "update_preferences_dispatched",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "update_preferences_late",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "update_preferences_picked_up",
                table: "customers");
        }
    }
}
