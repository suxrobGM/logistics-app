using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Application.Modules.Integrations.Agents.Services;

public static class AgentMessageSequences
{
    /// <summary>
    /// Reserves <paramref name="count"/> consecutive message sequences on a saved conversation in one
    /// atomic statement and returns the first. A turn, a decision note and a broker reply can append to
    /// the same conversation at once, so the counter must move in the database, never in memory.
    /// </summary>
    public static async Task<int> ReserveSequencesAsync(
        this ITenantUnitOfWork tenantUow, AgentConversation conversation, int count, CancellationToken ct)
    {
        var last = await tenantUow.SqlQueryAsync<int>($"""
            UPDATE agent_conversations SET last_sequence = last_sequence + {count}
            WHERE id = {conversation.Id}
            RETURNING last_sequence AS "Value"
            """, ct);
        return last.Single() - count + 1;
    }
}
