using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Master
{
    /// <inheritdoc />
    public partial class RequireWeeklyAIBudget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // A null budget used to mean unlimited. Adopt the Enterprise budget instead of the 0m
            // EF would default to - zero reads as "over quota from the first session".
            migrationBuilder.Sql(
                """
                UPDATE subscription_plans
                SET weekly_ai_budget_usd = COALESCE(
                    (SELECT weekly_ai_budget_usd FROM subscription_plans
                     WHERE tier = 'enterprise' AND weekly_ai_budget_usd IS NOT NULL
                     LIMIT 1),
                    75)
                WHERE weekly_ai_budget_usd IS NULL;
                """);

            migrationBuilder.AlterColumn<decimal>(
                name: "weekly_ai_budget_usd",
                table: "subscription_plans",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "weekly_ai_budget_usd",
                table: "subscription_plans",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);
        }
    }
}
