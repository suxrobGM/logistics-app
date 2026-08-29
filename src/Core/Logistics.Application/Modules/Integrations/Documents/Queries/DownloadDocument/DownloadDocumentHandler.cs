using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Logistics.Application.Abstractions.Storage;

namespace Logistics.Application.Modules.Integrations.Documents.Queries;

internal sealed class DownloadDocumentHandler(
    ITenantUnitOfWork tenantUow,
    IBlobStorageService blobStorageService)
    : IAppRequestHandler<DownloadDocumentQuery, Result<DocumentDownloadDto>>
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

        // Per-record authorization: without this any authenticated user could pull any document by
        // id (employee PII, all customers' BOLs/PODs). Management sees all; a driver only documents
        // tied to them; anyone else is denied here.
        if (!await DocumentAccess.CanAccessAsync(tenantUow, req.RequestedById, document, ct))
        {
            return Result<DocumentDownloadDto>.Fail("Document not found or access denied.");
        }

        try
        {
            var exists = await blobStorageService.ExistsAsync(document.BlobContainer, document.BlobPath, ct);
            if (!exists)
            {
                return Result<DocumentDownloadDto>.Fail("Document file not found in storage");
            }

            var stream = await blobStorageService.DownloadAsync(document.BlobContainer, document.BlobPath, ct);

            var dto = new DocumentDownloadDto
            {
                FileName = document.FileName,
                OriginalFileName = document.OriginalFileName,
                ContentType = document.ContentType,
                FileSizeBytes = document.FileSizeBytes,
                FileContent = stream
            };

            return Result<DocumentDownloadDto>.Ok(dto);
        }
        catch (Exception ex)
        {
            return Result<DocumentDownloadDto>.Fail($"Failed to download document: {ex.Message}");
        }
    }
}
