using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class RenameAgentSessionAndDecisionTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_ai_dispatch_decisions_ai_dispatch_session_session_id",
                table: "ai_dispatch_decisions");

            migrationBuilder.DropForeignKey(
                name: "fk_ai_dispatch_sessions_ai_copilot_conversations_conversation_",
                table: "ai_dispatch_sessions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_ai_dispatch_sessions",
                table: "ai_dispatch_sessions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_ai_dispatch_decisions",
                table: "ai_dispatch_decisions");

            migrationBuilder.RenameTable(
                name: "ai_dispatch_sessions",
                newName: "agent_sessions");

            migrationBuilder.RenameTable(
                name: "ai_dispatch_decisions",
                newName: "agent_decisions");

            migrationBuilder.RenameIndex(
                name: "ix_ai_dispatch_sessions_number",
                table: "agent_sessions",
                newName: "ix_agent_sessions_number");

            migrationBuilder.RenameIndex(
                name: "ix_ai_dispatch_sessions_conversation_id",
                table: "agent_sessions",
                newName: "ix_agent_sessions_conversation_id");

            migrationBuilder.RenameIndex(
                name: "ix_ai_dispatch_decisions_status",
                table: "agent_decisions",
                newName: "ix_agent_decisions_status");

            migrationBuilder.RenameIndex(
                name: "ix_ai_dispatch_decisions_session_id",
                table: "agent_decisions",
                newName: "ix_agent_decisions_session_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_agent_sessions",
                table: "agent_sessions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_agent_decisions",
                table: "agent_decisions",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_agent_decisions_agent_session_session_id",
                table: "agent_decisions",
                column: "session_id",
                principalTable: "agent_sessions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_agent_sessions_ai_copilot_conversation_conversation_id",
                table: "agent_sessions",
                column: "conversation_id",
                principalTable: "ai_copilot_conversations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_agent_decisions_agent_session_session_id",
                table: "agent_decisions");

            migrationBuilder.DropForeignKey(
                name: "fk_agent_sessions_ai_copilot_conversation_conversation_id",
                table: "agent_sessions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_agent_sessions",
                table: "agent_sessions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_agent_decisions",
                table: "agent_decisions");

            migrationBuilder.RenameTable(
                name: "agent_sessions",
                newName: "ai_dispatch_sessions");

            migrationBuilder.RenameTable(
                name: "agent_decisions",
                newName: "ai_dispatch_decisions");

            migrationBuilder.RenameIndex(
                name: "ix_agent_sessions_number",
                table: "ai_dispatch_sessions",
                newName: "ix_ai_dispatch_sessions_number");

            migrationBuilder.RenameIndex(
                name: "ix_agent_sessions_conversation_id",
                table: "ai_dispatch_sessions",
                newName: "ix_ai_dispatch_sessions_conversation_id");

            migrationBuilder.RenameIndex(
                name: "ix_agent_decisions_status",
                table: "ai_dispatch_decisions",
                newName: "ix_ai_dispatch_decisions_status");

            migrationBuilder.RenameIndex(
                name: "ix_agent_decisions_session_id",
                table: "ai_dispatch_decisions",
                newName: "ix_ai_dispatch_decisions_session_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_ai_dispatch_sessions",
                table: "ai_dispatch_sessions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_ai_dispatch_decisions",
                table: "ai_dispatch_decisions",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_ai_dispatch_decisions_ai_dispatch_session_session_id",
                table: "ai_dispatch_decisions",
                column: "session_id",
                principalTable: "ai_dispatch_sessions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_ai_dispatch_sessions_ai_copilot_conversations_conversation_",
                table: "ai_dispatch_sessions",
                column: "conversation_id",
                principalTable: "ai_copilot_conversations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
