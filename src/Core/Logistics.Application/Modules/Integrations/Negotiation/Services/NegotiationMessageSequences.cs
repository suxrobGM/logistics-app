using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Application.Modules.Integrations.Negotiation.Services;

public static class NegotiationMessageSequences
{
    /// <summary>
    /// Reserves the next message sequence on a saved negotiation in one atomic statement. A counter
    /// offer and an inbound broker reply can land on the same thread at once, so the counter must
    /// move in the database, never in memory.
    /// </summary>
    public static async Task<int> ReserveSequenceAsync(
        this ITenantUnitOfWork tenantUow, RateNegotiation negotiation, CancellationToken ct)
    {
        var last = await tenantUow.SqlQueryAsync<int>($"""
            UPDATE rate_negotiations SET last_sequence = last_sequence + 1
            WHERE id = {negotiation.Id}
            RETURNING last_sequence AS "Value"
            """, ct);
        return last.Single();
    }
}
