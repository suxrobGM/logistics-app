using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Logistics.Application.Queries;

internal sealed class GetTrackingLinksForLoadHandler(
    ITenantUnitOfWork tenantUow,
    IConfiguration configuration)
    : IAppRequestHandler<GetTrackingLinksForLoadQuery, Result<IEnumerable<TrackingLinkDto>>>
{
    public async Task<Result<IEnumerable<TrackingLinkDto>>> Handle(
        GetTrackingLinksForLoadQuery req,
        CancellationToken ct)
    {
        var load = await tenantUow.Repository<Load>().GetByIdAsync(req.LoadId, ct);
        if (load is null)
        {
            return Result<IEnumerable<TrackingLinkDto>>.Fail("Load not found.");
        }

        var tenant = tenantUow.GetCurrentTenant();
        var portalBaseUrl = configuration["CustomerPortal:BaseUrl"] ?? "http://localhost:7004";

        var trackingLinks = await tenantUow.Repository<TrackingLink>().Query()
            .Where(t => t.LoadId == req.LoadId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);

        var dtos = trackingLinks.Select(t => new TrackingLinkDto
        {
            Id = t.Id,
            Token = t.Token,
            Url = $"{portalBaseUrl}/track/{tenant.Id}/{t.Token}",
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
