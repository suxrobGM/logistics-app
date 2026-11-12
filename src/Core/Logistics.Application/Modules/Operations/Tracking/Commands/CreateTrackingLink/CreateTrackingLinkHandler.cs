using Logistics.Application.Modules.Operations.Tracking.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Tracking.Commands;

internal sealed class CreateTrackingLinkHandler(
    ITenantUnitOfWork tenantUow,
    ITrackingLinkService trackingLinks)
    : IRequestHandler<CreateTrackingLinkCommand, Result<TrackingLinkDto>>
{
    public async Task<Result<TrackingLinkDto>> Handle(CreateTrackingLinkCommand req, CancellationToken ct)
    {
        var load = await tenantUow.Repository<Load>().GetByIdAsync(req.LoadId, ct);
        if (load is null)
        {
            return Result<TrackingLinkDto>.Fail("Load not found.");
        }

        var trackingLink = await trackingLinks.GetOrCreateAsync(load.Id, ct);
        await tenantUow.SaveChangesAsync(ct);

        return Result<TrackingLinkDto>.Ok(new TrackingLinkDto
        {
            Id = trackingLink.Id,
            Token = trackingLink.Token,
            Url = trackingLinks.GetUrl(trackingLink),
            LoadId = load.Id,
            LoadNumber = load.Number,
            LoadName = load.Name,
            ExpiresAt = trackingLink.ExpiresAt,
            IsActive = trackingLink.IsActive,
            IsExpired = false,
            AccessCount = trackingLink.AccessCount,
            LastAccessedAt = trackingLink.LastAccessedAt,
            CreatedAt = trackingLink.CreatedAt
        });
    }
}
