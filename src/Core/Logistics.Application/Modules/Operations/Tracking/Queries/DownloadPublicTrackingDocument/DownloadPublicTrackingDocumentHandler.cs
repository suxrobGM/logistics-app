using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Operations.Tracking.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Logistics.Application.Modules.Integrations.Documents.Services;

namespace Logistics.Application.Modules.Operations.Tracking.Queries;

internal sealed class DownloadPublicTrackingDocumentHandler(
    ITrackingLinkService trackingLinkService,
    ITenantUnitOfWork tenantUow,
    IDocumentDownloadService documentDownloads)
    : IRequestHandler<DownloadPublicTrackingDocumentQuery, Result<DocumentDownloadDto>>
{
    public async Task<Result<DocumentDownloadDto>> Handle(
        DownloadPublicTrackingDocumentQuery req,
        CancellationToken ct)
    {
        var opened = await trackingLinkService.OpenPublicLinkAsync(req.TenantId, req.Token, ct);
        if (!opened.IsSuccess)
        {
            return Result<DocumentDownloadDto>.Fail(opened.Error!);
        }

        var trackingLink = opened.Value!;

        // Get the document and verify it belongs to the load
        var document = await tenantUow.Repository<LoadDocument>()
            .GetAsync(d => d.Id == req.DocumentId && d.LoadId == trackingLink.LoadId, ct);

        if (document is null)
        {
            return Result<DocumentDownloadDto>.Fail("Document not found.", ErrorCodes.NotFound);
        }

        return await documentDownloads.OpenAsync(document, ct);
    }
}
