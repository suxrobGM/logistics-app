using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddMessageSequenceCounters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "last_sequence",
                table: "rate_negotiations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "last_sequence",
                table: "agent_conversations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Existing threads already handed out sequences. Without this the counter restarts at 1
            // and the next message collides with one already stored.
            migrationBuilder.Sql("""
                UPDATE rate_negotiations n
                SET last_sequence = m.max_sequence
                FROM (
                    SELECT negotiation_id, MAX(sequence) AS max_sequence
                    FROM negotiation_messages
                    GROUP BY negotiation_id
                ) m
                WHERE m.negotiation_id = n.id;
                """);

            migrationBuilder.Sql("""
                UPDATE agent_conversations c
                SET last_sequence = m.max_sequence
                FROM (
                    SELECT conversation_id, MAX(sequence) AS max_sequence
                    FROM agent_messages
                    GROUP BY conversation_id
                ) m
                WHERE m.conversation_id = c.id;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "last_sequence",
                table: "rate_negotiations");

            migrationBuilder.DropColumn(
                name: "last_sequence",
                table: "agent_conversations");
        }
    }
}
