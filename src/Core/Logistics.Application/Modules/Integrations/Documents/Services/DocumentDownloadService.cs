using Logistics.Application.Abstractions.Storage;
using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Integrations.Documents.Services;

internal sealed class DocumentDownloadService(
    IBlobStorageService blobStorage,
    ILogger<DocumentDownloadService> logger) : IDocumentDownloadService
{
    public async Task<Result<DocumentDownloadDto>> OpenAsync(Document document, CancellationToken ct = default)
    {
        try
        {
            if (!await blobStorage.ExistsAsync(document.BlobContainer, document.BlobPath, ct))
            {
                return Result<DocumentDownloadDto>.Fail("Document file not found in storage.");
            }

            return Result<DocumentDownloadDto>.Ok(new DocumentDownloadDto
            {
                FileName = document.FileName,
                OriginalFileName = document.OriginalFileName,
                ContentType = document.ContentType,
                FileSizeBytes = document.FileSizeBytes,
                FileContent = await blobStorage.DownloadAsync(document.BlobContainer, document.BlobPath, ct)
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to download document {DocumentId}", document.Id);
            return Result<DocumentDownloadDto>.Fail("Failed to download document.");
        }
    }
}
