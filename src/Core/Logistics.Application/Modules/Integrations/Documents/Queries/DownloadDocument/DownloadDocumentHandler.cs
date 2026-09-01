using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.Storage;

namespace Logistics.Application.Modules.Integrations.Documents.Queries;

internal sealed class DownloadDocumentHandler(
    ITenantUnitOfWork tenantUow,
    IBlobStorageService blobStorageService,
    ICurrentUserService currentUserService,
    ILogger<DownloadDocumentHandler> logger)
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

        var access = await DocumentAccess.ResolveAsync(tenantUow, currentUserService, ct);
        if (access is null || !await DocumentAccess.CanAccessAsync(tenantUow, access, document, ct))
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
            logger.LogError(ex, "Failed to download document {DocumentId}", req.DocumentId);
            return Result<DocumentDownloadDto>.Fail("Failed to download document.");
        }
    }
}
