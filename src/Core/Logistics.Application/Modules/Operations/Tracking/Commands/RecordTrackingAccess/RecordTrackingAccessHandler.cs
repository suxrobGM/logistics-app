using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Operations.Tracking.Commands;

internal sealed class RecordTrackingAccessHandler(
    ITenantUnitOfWork tenantUow,
    ILogger<RecordTrackingAccessHandler> logger)
    : IAppRequestHandler<RecordTrackingAccessCommand, Result>
{
    public async Task<Result> Handle(RecordTrackingAccessCommand req, CancellationToken ct)
    {
        try
        {
            await tenantUow.SetCurrentTenantByIdAsync(req.TenantId);
        }
        catch (InvalidOperationException)
        {
            return Result.Fail("Invalid tenant.");
        }

        var trackingLink = await tenantUow.Repository<TrackingLink>().GetByIdAsync(req.TrackingLinkId, ct);
        if (trackingLink is null)
        {
            logger.LogWarning(
                "RecordTrackingAccess: tracking link {LinkId} not found for tenant {TenantId}.",
                req.TrackingLinkId, req.TenantId);
            return Result.Fail("Tracking link not found.");
        }

        trackingLink.RecordAccess();
        tenantUow.Repository<TrackingLink>().Update(trackingLink);
        await tenantUow.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
