using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class Version_0006 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "total_tokens_used",
                table: "dispatch_sessions",
                newName: "output_tokens_used");

            migrationBuilder.AddColumn<int>(
                name: "cache_creation_tokens",
                table: "dispatch_sessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "cache_read_tokens",
                table: "dispatch_sessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "estimated_cost_usd",
                table: "dispatch_sessions",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "input_tokens_used",
                table: "dispatch_sessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "instructions",
                table: "dispatch_sessions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "model_used",
                table: "dispatch_sessions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cache_creation_tokens",
                table: "dispatch_sessions");

            migrationBuilder.DropColumn(
                name: "cache_read_tokens",
                table: "dispatch_sessions");

            migrationBuilder.DropColumn(
                name: "estimated_cost_usd",
                table: "dispatch_sessions");

            migrationBuilder.DropColumn(
                name: "input_tokens_used",
                table: "dispatch_sessions");

            migrationBuilder.DropColumn(
                name: "instructions",
                table: "dispatch_sessions");

            migrationBuilder.DropColumn(
                name: "model_used",
                table: "dispatch_sessions");

            migrationBuilder.RenameColumn(
                name: "output_tokens_used",
                table: "dispatch_sessions",
                newName: "total_tokens_used");
        }
    }
}
