using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class RemovePostedTruckHandler(
    ITenantUnitOfWork tenantUow,
    ILoadBoardProviderFactory providerFactory,
    ILogger<RemovePostedTruckHandler> logger)
    : IAppRequestHandler<RemovePostedTruckCommand, Result>
{
    public async Task<Result> Handle(RemovePostedTruckCommand req, CancellationToken ct)
    {
        var postedTruck = await tenantUow.Repository<PostedTruck>().GetByIdAsync(req.PostedTruckId, ct);
        if (postedTruck is null)
        {
            return Result.Fail("Posted truck not found");
        }

        if (postedTruck.Status == PostedTruckStatus.Expired)
        {
            // Already expired, just delete the record
            tenantUow.Repository<PostedTruck>().Delete(postedTruck);
            await tenantUow.SaveChangesAsync(ct);
            return Result.Ok();
        }

        // Get provider configuration
        var providerConfig = await tenantUow.Repository<LoadBoardConfiguration>()
            .GetAsync(c => c.ProviderType == postedTruck.ProviderType && c.IsActive, ct);

        if (providerConfig is not null && !string.IsNullOrEmpty(postedTruck.ExternalPostId))
        {
            // Try to remove from the load board provider
            var provider = providerFactory.GetProvider(providerConfig);
            var removed = await provider.RemoveTruckPostAsync(postedTruck.ExternalPostId);

            if (!removed)
            {
                logger.LogWarning(
                    "Failed to remove truck post {ExternalPostId} from {Provider}, removing local record anyway",
                    postedTruck.ExternalPostId, postedTruck.ProviderType);
            }
        }

        // Delete the local record
        tenantUow.Repository<PostedTruck>().Delete(postedTruck);
        await tenantUow.SaveChangesAsync(ct);

        logger.LogInformation(
            "Removed posted truck {PostedTruckId} (external: {ExternalPostId}) from {Provider}",
            postedTruck.Id, postedTruck.ExternalPostId, postedTruck.ProviderType);

        return Result.Ok();
    }
}
