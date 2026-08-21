using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Negotiation.Commands;

internal sealed class CloseNegotiationHandler(
    ITenantUnitOfWork tenantUow,
    IMasterUnitOfWork masterUow,
    IAIDispatchBroadcastService broadcastService)
    : IAppRequestHandler<CloseNegotiationCommand, Result>
{
    public async Task<Result> Handle(CloseNegotiationCommand req, CancellationToken ct)
    {
        var negotiation = await tenantUow.Repository<RateNegotiation>().GetByIdAsync(req.Id, ct);
        if (negotiation is null)
        {
            return Result.Fail($"Could not find a negotiation with ID '{req.Id}'");
        }

        if (negotiation.Status is not (RateNegotiationStatus.AwaitingBroker or RateNegotiationStatus.BrokerReplied))
        {
            return Result.Fail($"This negotiation is already {negotiation.Status.GetDescription().ToLowerInvariant()}.");
        }

        negotiation.Close(
            req.Declined ? RateNegotiationStatus.Declined : RateNegotiationStatus.Closed,
            req.Reason);

        await tenantUow.SaveChangesAsync(ct);

        var route = await masterUow.Repository<InboundEmailRoute>()
            .GetAsync(r => r.ThreadToken == negotiation.ReplyToken, ct);

        if (route is { RevokedAt: null })
        {
            route.RevokedAt = DateTime.UtcNow;
            await masterUow.SaveChangesAsync(ct);
        }

        var listing = await tenantUow.Repository<LoadBoardListing>()
            .GetByIdAsync(negotiation.LoadBoardListingId, ct);

        await broadcastService.BroadcastNegotiationAsync(
            tenantUow.GetCurrentTenant().Id, negotiation.ToDto(listing));

        return Result.Ok();
    }
}
