using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Master
{
    /// <inheritdoc />
    public partial class Version_0008 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "preferred_language",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "settings_language",
                table: "tenants",
                type: "text",
                nullable: false,
                defaultValue: "en");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "preferred_language",
                table: "users");

            migrationBuilder.DropColumn(
                name: "settings_language",
                table: "tenants");
        }
    }
}
