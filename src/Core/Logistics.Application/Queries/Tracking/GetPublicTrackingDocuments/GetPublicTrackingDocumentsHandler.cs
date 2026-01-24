using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Queries;

internal sealed class GetPublicTrackingDocumentsHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetPublicTrackingDocumentsQuery, Result<IEnumerable<DocumentDto>>>
{
    public async Task<Result<IEnumerable<DocumentDto>>> Handle(
        GetPublicTrackingDocumentsQuery req,
        CancellationToken ct)
    {
        // Set tenant context for the query
        try
        {
            await tenantUow.SetCurrentTenantByIdAsync(req.TenantId);
        }
        catch (InvalidOperationException)
        {
            return Result<IEnumerable<DocumentDto>>.Fail("Invalid tracking link.");
        }

        // Find the tracking link
        var trackingLink = await tenantUow.Repository<TrackingLink>()
            .GetAsync(t => t.Token == req.Token, ct);

        if (trackingLink is null)
        {
            return Result<IEnumerable<DocumentDto>>.Fail("Tracking link not found.");
        }

        if (!trackingLink.IsValid)
        {
            return Result<IEnumerable<DocumentDto>>.Fail("This tracking link has expired or been revoked.");
        }

        // Get documents for the load
        var documents = await tenantUow.Repository<LoadDocument>().Query()
            .Where(d => d.LoadId == trackingLink.LoadId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(ct);

        var dtos = documents.Select(d => d.ToDto());
        return Result<IEnumerable<DocumentDto>>.Ok(dtos);
    }
}
