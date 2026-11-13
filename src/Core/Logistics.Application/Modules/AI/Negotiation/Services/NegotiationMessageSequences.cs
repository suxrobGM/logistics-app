using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Application.Modules.AI.Negotiation.Services;

public static class NegotiationMessageSequences
{
    extension(ITenantUnitOfWork tenantUow)
    {
        /// <summary>
        /// Reserves the next message number in one database update, so a counter offer and a broker reply arriving
        /// at once never get the same number.
        /// </summary>
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
