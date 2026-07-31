using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddAgentSessionStartedAtIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_agent_sessions_started_at",
                table: "agent_sessions",
                column: "started_at")
                .Annotation("Npgsql:IndexInclude", new[] { "estimated_cost_usd" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_agent_sessions_started_at",
                table: "agent_sessions");
        }
    }
}
