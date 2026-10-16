using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <summary>
    /// Agent turns used to advance <c>last_sequence</c> by one while writing several messages, and the
    /// squashed migration that added the column never backfilled it. Either way the next appended
    /// message collides on <c>ix_agent_messages_conversation_id_sequence</c>; catch the counter up.
    /// </summary>
    public partial class RepairConversationLastSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE agent_conversations c
                SET last_sequence = m.max_sequence
                FROM (
                    SELECT conversation_id, MAX(sequence) AS max_sequence
                    FROM agent_messages
                    GROUP BY conversation_id
                ) m
                WHERE m.conversation_id = c.id AND c.last_sequence < m.max_sequence;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
