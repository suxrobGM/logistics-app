using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Application.Modules.AI.Agents.Services;

public static class AgentMessageSequences
{
    extension(ITenantUnitOfWork tenantUow)
    {
        /// <summary>
        /// Reserves <paramref name="count"/> message numbers in one database update and returns the first, so two
        /// turns writing at once never get the same number.
        /// </summary>
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
