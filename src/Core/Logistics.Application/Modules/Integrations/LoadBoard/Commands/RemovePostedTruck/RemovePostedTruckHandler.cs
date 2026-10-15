using Logistics.Application.Modules.Integrations.LoadBoard.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Integrations.LoadBoard.Commands;

internal sealed class RemovePostedTruckHandler(
    ITenantUnitOfWork tenantUow,
    ILoadBoardProviderGateway gateway,
    ILogger<RemovePostedTruckHandler> logger)
    : IRequestHandler<RemovePostedTruckCommand, Result>
{
    public async Task<Result> Handle(RemovePostedTruckCommand req, CancellationToken ct)
    {
        var postedTruck = await tenantUow.Repository<PostedTruck>().GetByIdAsync(req.PostedTruckId, ct);
        if (postedTruck is null)
        {
            return Result.Fail("Posted truck not found");
        }

        var providerConfig = await tenantUow.Repository<LoadBoardConfiguration>()
            .GetAsync(c => c.ProviderType == postedTruck.ProviderType && c.IsActive, ct);

        // A post still live on the board must come off it first, or brokers keep calling about a
        // truck that is gone. Without an active configuration there is no way to reach the board.
        if (postedTruck.Status == PostedTruckStatus.Available &&
            providerConfig is not null &&
            postedTruck.ExternalPostId is { Length: > 0 } externalPostId)
        {
            var removed = await gateway.ExecuteAsync(
                providerConfig, p => p.RemoveTruckPostAsync(externalPostId, ct), ct);
            if (!removed.IsSuccess)
            {
                return removed;
            }
        }

        tenantUow.Repository<PostedTruck>().Delete(postedTruck);
        await tenantUow.SaveChangesAsync(ct);

        logger.LogInformation(
            "Removed posted truck {PostedTruckId} (external: {ExternalPostId}) from {Provider}",
            postedTruck.Id, postedTruck.ExternalPostId, postedTruck.ProviderType);

        return Result.Ok();
    }
}
