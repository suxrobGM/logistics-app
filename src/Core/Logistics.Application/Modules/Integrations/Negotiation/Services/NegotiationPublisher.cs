using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;

namespace Logistics.Application.Modules.Integrations.Negotiation.Services;

internal sealed class NegotiationPublisher(
    ITenantUnitOfWork tenantUow,
    IAIDispatchBroadcastService broadcastService) : INegotiationPublisher
{
    public async Task PublishAsync(RateNegotiation negotiation, CancellationToken ct)
    {
        var listing = await tenantUow.Repository<LoadBoardListing>()
            .GetByIdAsync(negotiation.LoadBoardListingId, ct);

        await broadcastService.BroadcastNegotiationAsync(
            tenantUow.GetCurrentTenant().Id, negotiation.ToDto(listing));
    }
}
