using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.EntityFramework.DbMigrations.Migrations
{
    public partial class AddUserNameField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "User",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserName",
                table: "User");
        }
    }
}
