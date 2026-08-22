using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;

namespace Logistics.Application.Modules.Integrations.Negotiation.Services;

/// <summary>
/// Pushes a single thread to the tenant's dispatch clients after a state change.
/// </summary>
internal static class NegotiationBroadcast
{
    /// <summary>
    /// Fetches the thread's listing and broadcasts the mapped row. The listing is read explicitly
    /// rather than off the navigation because the mapper takes it as an argument precisely so the
    /// lazy-loaded navigation is never touched. Callers that already hold the listing should map and
    /// broadcast directly instead of paying for a second read.
    /// </summary>
    public static async Task PublishAsync(
        ITenantUnitOfWork tenantUow,
        IAIDispatchBroadcastService broadcastService,
        RateNegotiation negotiation,
        CancellationToken ct)
    {
        var listing = await tenantUow.Repository<LoadBoardListing>()
            .GetByIdAsync(negotiation.LoadBoardListingId, ct);

        await broadcastService.BroadcastNegotiationAsync(
            tenantUow.GetCurrentTenant().Id, negotiation.ToDto(listing));
    }
}
