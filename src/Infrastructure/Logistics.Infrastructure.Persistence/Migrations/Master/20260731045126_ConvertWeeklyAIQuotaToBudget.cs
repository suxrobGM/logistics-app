using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Master
{
    /// <summary>
    /// Converts the per-plan weekly AI quota from request units to a USD cost budget, and the
    /// extended-thinking flag to the reasoning-effort level. Hand-edited: EF scaffolds the column
    /// change as Drop+Add, which would wipe every plan's quota. The 0.015 factor is the estimated
    /// real model cost of one legacy request unit; the seeder then rounds the three canonical
    /// plans to their curated budgets.
    /// </summary>
    public partial class ConvertWeeklyAIQuotaToBudget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "weekly_ai_request_quota",
                table: "subscription_plans",
                newName: "weekly_ai_budget_usd");

            migrationBuilder.AlterColumn<decimal>(
                name: "weekly_ai_budget_usd",
                table: "subscription_plans",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.Sql("""
                UPDATE subscription_plans
                SET weekly_ai_budget_usd = ROUND(weekly_ai_budget_usd * 0.015, 2)
                WHERE weekly_ai_budget_usd IS NOT NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE system_settings
                SET key = 'AI.ReasoningEffort',
                    value = CASE WHEN value = 'True' THEN 'High' ELSE 'None' END
                WHERE key = 'AI.ExtendedThinking'
                  AND NOT EXISTS (SELECT 1 FROM system_settings WHERE key = 'AI.ReasoningEffort');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE system_settings
                SET key = 'AI.ExtendedThinking',
                    value = CASE WHEN value = 'None' THEN 'False' ELSE 'True' END
                WHERE key = 'AI.ReasoningEffort'
                  AND NOT EXISTS (SELECT 1 FROM system_settings WHERE key = 'AI.ExtendedThinking');
                """);

            migrationBuilder.Sql("""
                UPDATE subscription_plans
                SET weekly_ai_budget_usd = ROUND(weekly_ai_budget_usd / 0.015)
                WHERE weekly_ai_budget_usd IS NOT NULL;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "weekly_ai_budget_usd",
                table: "subscription_plans",
                type: "integer",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.RenameColumn(
                name: "weekly_ai_budget_usd",
                table: "subscription_plans",
                newName: "weekly_ai_request_quota");
        }
    }
}
