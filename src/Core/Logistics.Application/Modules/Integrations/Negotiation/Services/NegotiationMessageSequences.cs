using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Application.Modules.Integrations.Negotiation.Services;

public static class NegotiationMessageSequences
{
    extension(ITenantUnitOfWork tenantUow)
    {
        /// <summary>Reserved atomically in the database, since a counter offer and a broker reply can race.</summary>
        public async Task<int> ReserveSequenceAsync(
            RateNegotiation negotiation, CancellationToken ct)
        {
            var last = await tenantUow.SqlQueryAsync<int>($"""
                    UPDATE rate_negotiations SET last_sequence = last_sequence + 1
                    WHERE id = {negotiation.Id}
                    RETURNING last_sequence AS "Value"
                    """, ct);
            return last.Single();
        }
    }
}
