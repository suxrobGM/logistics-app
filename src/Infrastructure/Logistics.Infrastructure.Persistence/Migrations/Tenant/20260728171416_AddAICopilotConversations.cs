using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddAICopilotConversations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "conversation_id",
                table: "ai_dispatch_sessions",
                type: "uuid",
                nullable: true);

            // Existing rows predate the discriminator and are all dispatch runs; an empty-string
            // backfill would fail enum parsing on first read.
            migrationBuilder.AddColumn<string>(
                name: "type",
                table: "ai_dispatch_sessions",
                type: "text",
                nullable: false,
                defaultValue: "dispatch");

            migrationBuilder.AddColumn<Guid>(
                name: "customer_id",
                table: "ai_dispatch_decisions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "invoice_id",
                table: "ai_dispatch_decisions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ai_copilot_conversations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    turn_started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_message_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ai_copilot_conversations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ai_copilot_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    conversation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequence = table.Column<int>(type: "integer", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    content_json = table.Column<string>(type: "text", nullable: false),
                    display_text = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    session_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ai_copilot_messages", x => x.id);
                    table.ForeignKey(
                        name: "fk_ai_copilot_messages_ai_copilot_conversations_conversation_id",
                        column: x => x.conversation_id,
                        principalTable: "ai_copilot_conversations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_ai_dispatch_sessions_conversation_id",
                table: "ai_dispatch_sessions",
                column: "conversation_id");

            migrationBuilder.CreateIndex(
                name: "ix_ai_copilot_conversations_created_by_id",
                table: "ai_copilot_conversations",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_ai_copilot_messages_conversation_id_sequence",
                table: "ai_copilot_messages",
                columns: new[] { "conversation_id", "sequence" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_ai_dispatch_sessions_ai_copilot_conversations_conversation_",
                table: "ai_dispatch_sessions",
                column: "conversation_id",
                principalTable: "ai_copilot_conversations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_ai_dispatch_sessions_ai_copilot_conversations_conversation_",
                table: "ai_dispatch_sessions");

            migrationBuilder.DropTable(
                name: "ai_copilot_messages");

            migrationBuilder.DropTable(
                name: "ai_copilot_conversations");

            migrationBuilder.DropIndex(
                name: "ix_ai_dispatch_sessions_conversation_id",
                table: "ai_dispatch_sessions");

            migrationBuilder.DropColumn(
                name: "conversation_id",
                table: "ai_dispatch_sessions");

            migrationBuilder.DropColumn(
                name: "type",
                table: "ai_dispatch_sessions");

            migrationBuilder.DropColumn(
                name: "customer_id",
                table: "ai_dispatch_decisions");

            migrationBuilder.DropColumn(
                name: "invoice_id",
                table: "ai_dispatch_decisions");
        }
    }
}
