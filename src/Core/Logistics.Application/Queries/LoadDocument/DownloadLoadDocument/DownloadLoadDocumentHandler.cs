using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Services;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class DownloadLoadDocumentHandler : RequestHandler<DownloadLoadDocumentQuery, Result<LoadDocumentDownloadDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;
    private readonly IBlobStorageService _blobStorageService;

    public DownloadLoadDocumentHandler(
        ITenantUnityOfWork tenantUow,
        IBlobStorageService blobStorageService)
    {
        _tenantUow = tenantUow;
        _blobStorageService = blobStorageService;
    }

    protected override async Task<Result<LoadDocumentDownloadDto>> HandleValidated(
        DownloadLoadDocumentQuery req, CancellationToken cancellationToken)
    {
        // Get the document
        var document = await _tenantUow.Repository<LoadDocument>().GetByIdAsync(req.DocumentId);
        if (document is null)
        {
            return Result<LoadDocumentDownloadDto>.Fail($"Could not find document with ID '{req.DocumentId}'");
        }

        // Check if document is deleted
        if (document.Status == DocumentStatus.Deleted)
        {
            return Result<LoadDocumentDownloadDto>.Fail("Document has been deleted");
        }

        // Verify requester exists (for audit purposes)
        var requester = await _tenantUow.Repository<Employee>().GetByIdAsync(req.RequestedById);
        if (requester is null)
        {
            return Result<LoadDocumentDownloadDto>.Fail($"Could not find employee with ID '{req.RequestedById}'");
        }

        try
        {
            // Check if blob exists
            var blobExists = await _blobStorageService.ExistsAsync(document.BlobContainer, document.BlobPath, cancellationToken);
            if (!blobExists)
            {
                return Result<LoadDocumentDownloadDto>.Fail("Document file not found in storage");
            }

            // Download from blob storage
            var fileStream = await _blobStorageService.DownloadAsync(document.BlobContainer, document.BlobPath, cancellationToken);

            var downloadDto = new LoadDocumentDownloadDto
            {
                FileName = document.FileName,
                OriginalFileName = document.OriginalFileName,
                ContentType = document.ContentType,
                FileSizeBytes = document.FileSizeBytes,
                FileContent = fileStream
            };

            return Result<LoadDocumentDownloadDto>.Succeed(downloadDto);
        }
        catch (Exception ex)
        {
            return Result<LoadDocumentDownloadDto>.Fail($"Failed to download document: {ex.Message}");
        }
    }
}
