using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Operations.Tracking.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Operations.Tracking.Queries;

internal sealed class GetTrackingLinksForLoadHandler(
    ITenantUnitOfWork tenantUow,
    ITrackingLinkService trackingLinkService)
    : IRequestHandler<GetTrackingLinksForLoadQuery, Result<IEnumerable<TrackingLinkDto>>>
{
    public async Task<Result<IEnumerable<TrackingLinkDto>>> Handle(
        GetTrackingLinksForLoadQuery req,
        CancellationToken ct)
    {
        var load = await tenantUow.Repository<Load>().GetByIdAsync(req.LoadId, ct);
        if (load is null)
        {
            return Result<IEnumerable<TrackingLinkDto>>.Fail("Load not found.", ErrorCodes.NotFound);
        }

        var trackingLinks = await tenantUow.Repository<TrackingLink>().Query()
            .Where(t => t.LoadId == req.LoadId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);

        var dtos = trackingLinks.Select(t => new TrackingLinkDto
        {
            Id = t.Id,
            Token = t.Token,
            Url = trackingLinkService.GetUrl(t),
            LoadId = load.Id,
            LoadNumber = load.Number,
            LoadName = load.Name,
            ExpiresAt = t.ExpiresAt,
            IsActive = t.IsActive,
            IsExpired = DateTime.UtcNow >= t.ExpiresAt,
            AccessCount = t.AccessCount,
            LastAccessedAt = t.LastAccessedAt,
            CreatedAt = t.CreatedAt
        });

        return Result<IEnumerable<TrackingLinkDto>>.Ok(dtos);
    }
}
