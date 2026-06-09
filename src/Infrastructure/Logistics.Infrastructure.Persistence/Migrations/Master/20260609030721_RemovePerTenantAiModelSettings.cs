using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Master
{
    /// <inheritdoc />
    public partial class RemovePerTenantAiModelSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "settings_llm_extended_thinking",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "settings_llm_model",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "settings_llm_provider",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "allowed_model_tier",
                table: "subscription_plans");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "settings_llm_extended_thinking",
                table: "tenants",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "settings_llm_model",
                table: "tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "settings_llm_provider",
                table: "tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "allowed_model_tier",
                table: "subscription_plans",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
