using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Operations.Tracking.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Operations.Tracking.Queries;

internal sealed class GetPublicTrackingDocumentsHandler(
    ITrackingLinkService trackingLinkService,
    ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetPublicTrackingDocumentsQuery, Result<IEnumerable<DocumentDto>>>
{
    public async Task<Result<IEnumerable<DocumentDto>>> Handle(
        GetPublicTrackingDocumentsQuery req,
        CancellationToken ct)
    {
        var opened = await trackingLinkService.OpenPublicLinkAsync(req.TenantId, req.Token, ct);
        if (!opened.IsSuccess)
        {
            return Result<IEnumerable<DocumentDto>>.Fail(opened);
        }

        var trackingLink = opened.Value!;

        // Get documents for the load
        var documents = await tenantUow.Repository<LoadDocument>().Query()
            .Where(d => d.LoadId == trackingLink.LoadId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(ct);

        var dtos = documents.Select(d => d.ToDto());
        return Result<IEnumerable<DocumentDto>>.Ok(dtos);
    }
}
