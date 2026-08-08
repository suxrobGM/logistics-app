using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class RenameCopilotConversationsToAgentConversations : Migration
    {
        // Hand-written: the scaffold produced DropTable/CreateTable, which would destroy existing
        // copilot transcripts. Everything here is a metadata-only rename plus one new column.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "ai_copilot_conversations",
                newName: "agent_conversations");

            migrationBuilder.RenameTable(
                name: "ai_copilot_messages",
                newName: "agent_messages");

            migrationBuilder.Sql("""
                ALTER TABLE agent_conversations RENAME CONSTRAINT pk_ai_copilot_conversations TO pk_agent_conversations;
                ALTER TABLE agent_messages RENAME CONSTRAINT pk_ai_copilot_messages TO pk_agent_messages;
                ALTER TABLE agent_messages RENAME CONSTRAINT fk_ai_copilot_messages_ai_copilot_conversations_conversation_id TO fk_agent_messages_agent_conversations_conversation_id;
                ALTER TABLE agent_sessions RENAME CONSTRAINT fk_agent_sessions_ai_copilot_conversation_conversation_id TO fk_agent_sessions_agent_conversations_conversation_id;
                """);

            migrationBuilder.RenameIndex(
                name: "ix_ai_copilot_conversations_created_by_id",
                newName: "ix_agent_conversations_created_by_id",
                table: "agent_conversations");

            migrationBuilder.RenameIndex(
                name: "ix_ai_copilot_messages_conversation_id_sequence",
                newName: "ix_agent_messages_conversation_id_sequence",
                table: "agent_messages");

            migrationBuilder.AddColumn<string>(
                name: "kind",
                table: "agent_conversations",
                type: "text",
                nullable: false,
                defaultValue: "copilot");

            // The model declares no DB default; the AddColumn default only backfills existing rows.
            migrationBuilder.Sql("ALTER TABLE agent_conversations ALTER COLUMN kind DROP DEFAULT;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "kind",
                table: "agent_conversations");

            migrationBuilder.RenameIndex(
                name: "ix_agent_conversations_created_by_id",
                newName: "ix_ai_copilot_conversations_created_by_id",
                table: "agent_conversations");

            migrationBuilder.RenameIndex(
                name: "ix_agent_messages_conversation_id_sequence",
                newName: "ix_ai_copilot_messages_conversation_id_sequence",
                table: "agent_messages");

            migrationBuilder.Sql("""
                ALTER TABLE agent_conversations RENAME CONSTRAINT pk_agent_conversations TO pk_ai_copilot_conversations;
                ALTER TABLE agent_messages RENAME CONSTRAINT pk_agent_messages TO pk_ai_copilot_messages;
                ALTER TABLE agent_messages RENAME CONSTRAINT fk_agent_messages_agent_conversations_conversation_id TO fk_ai_copilot_messages_ai_copilot_conversations_conversation_id;
                ALTER TABLE agent_sessions RENAME CONSTRAINT fk_agent_sessions_agent_conversations_conversation_id TO fk_agent_sessions_ai_copilot_conversation_conversation_id;
                """);

            migrationBuilder.RenameTable(
                name: "agent_conversations",
                newName: "ai_copilot_conversations");

            migrationBuilder.RenameTable(
                name: "agent_messages",
                newName: "ai_copilot_messages");
        }
    }
}
