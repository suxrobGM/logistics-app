using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class DropAgentSessionNumberAndRequireConversation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Conversation-less sessions belong to the deleted run-based dispatch feature and are
            // unreachable from any surface. Their decisions cascade off agent_decisions.session_id.
            migrationBuilder.Sql("DELETE FROM agent_sessions WHERE conversation_id IS NULL;");

            migrationBuilder.AlterColumn<Guid>(
                name: "conversation_id",
                table: "agent_sessions",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.DropIndex(
                name: "ix_agent_sessions_number",
                table: "agent_sessions");

            migrationBuilder.DropColumn(
                name: "number",
                table: "agent_sessions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "conversation_id",
                table: "agent_sessions",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<long>(
                name: "number",
                table: "agent_sessions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);

            migrationBuilder.CreateIndex(
                name: "ix_agent_sessions_number",
                table: "agent_sessions",
                column: "number",
                unique: true);
        }
    }
}
