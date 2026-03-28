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
            migrationBuilder.RenameColumn(
                name: "weekly_ai_session_quota",
                table: "subscription_plans",
                newName: "weekly_ai_request_quota");

            migrationBuilder.AddColumn<int>(
                name: "settings_llm_provider",
                table: "tenants",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "allowed_model_tier",
                table: "subscription_plans",
                type: "text",
                nullable: false,
                defaultValue: "base");

            // Fix existing rows that may have empty string from a prior run
            migrationBuilder.Sql(
                "UPDATE subscription_plans SET allowed_model_tier = 'base' WHERE allowed_model_tier = ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "settings_llm_provider",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "allowed_model_tier",
                table: "subscription_plans");

            migrationBuilder.RenameColumn(
                name: "weekly_ai_request_quota",
                table: "subscription_plans",
                newName: "weekly_ai_session_quota");
        }
    }
}
