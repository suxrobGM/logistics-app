using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Application.Modules.Integrations.Agents.Services;

public static class AgentMessageSequences
{
    extension(ITenantUnitOfWork tenantUow)
    {
        /// <summary>Returns the first of <paramref name="count"/> sequences, reserved atomically because writers race.</summary>
        public async Task<int> ReserveSequencesAsync(
            AgentConversation conversation, int count, CancellationToken ct)
        {
            var last = await tenantUow.SqlQueryAsync<int>($"""
                    UPDATE agent_conversations SET last_sequence = last_sequence + {count}
                    WHERE id = {conversation.Id}
                    RETURNING last_sequence AS "Value"
                    """, ct);
            return last.Single() - count + 1;
        }
    }
}
