using Logistics.Application.Modules.Integrations.Documents.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.Portal.Queries;

internal sealed class DownloadPortalLoadDocumentHandler(
    ITenantUnitOfWork tenantUow,
    IDocumentDownloadService documentDownloads)
    : IRequestHandler<DownloadPortalLoadDocumentQuery, Result<DocumentDownloadDto>>
{
    private const string NotFound = "Document not found or access denied.";

    public async Task<Result<DocumentDownloadDto>> Handle(DownloadPortalLoadDocumentQuery req, CancellationToken ct)
    {
        var load = await tenantUow.Repository<Load>().GetByIdAsync(req.LoadId, ct);
        if (load is null || load.CustomerId != req.CustomerId)
        {
            return Result<DocumentDownloadDto>.Fail(NotFound);
        }

        var document = await tenantUow.Repository<LoadDocument>().GetByIdAsync(req.DocumentId, ct);
        if (document is null || document.LoadId != req.LoadId || document.Status == DocumentStatus.Deleted)
        {
            return Result<DocumentDownloadDto>.Fail(NotFound);
        }

        return await documentDownloads.OpenAsync(document, ct);
    }
}
