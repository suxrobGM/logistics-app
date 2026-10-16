using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Integrations.Documents.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Documents.Queries;

internal sealed class DownloadDocumentHandler(
    ITenantUnitOfWork tenantUow,
    IDocumentAccessService documentAccess,
    IDocumentDownloadService documentDownloads)
    : IRequestHandler<DownloadDocumentQuery, Result<DocumentDownloadDto>>
{
    public async Task<Result<DocumentDownloadDto>> Handle(DownloadDocumentQuery req, CancellationToken ct)
    {
        var document = await tenantUow.Repository<Document>().GetByIdAsync(req.DocumentId, ct);
        if (document is null)
        {
            return Result<DocumentDownloadDto>.Fail($"Could not find document with ID '{req.DocumentId}'");
        }

        if (document.Status == DocumentStatus.Deleted)
        {
            return Result<DocumentDownloadDto>.Fail("Document has been deleted");
        }

        var caller = await documentAccess.ResolveCallerAsync(ct);
        if (caller is null || !await documentAccess.CanAccessAsync(caller, document, ct))
        {
            return Result<DocumentDownloadDto>.Fail("Document not found or access denied.");
        }

        return await documentDownloads.OpenAsync(document, ct);
    }
}
