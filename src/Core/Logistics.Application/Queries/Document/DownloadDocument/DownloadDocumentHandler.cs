using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Services;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class
    DownloadDocumentHandler : RequestHandler<DownloadDocumentQuery, Result<DocumentDownloadDto>>
{
    private readonly IBlobStorageService _blobStorage;
    private readonly ITenantUnitOfWork _tenantUow;

    public DownloadDocumentHandler(
        ITenantUnitOfWork tenantUow,
        IBlobStorageService blobStorageService)
    {
        _tenantUow = tenantUow;
        _blobStorage = blobStorageService;
    }

    public override async Task<Result<DocumentDownloadDto>> Handle(DownloadDocumentQuery req, CancellationToken ct)
    {
        var document = await _tenantUow.Repository<Document>().GetByIdAsync(req.DocumentId, ct);
        if (document is null)
        {
            return Result<DocumentDownloadDto>.Fail($"Could not find document with ID '{req.DocumentId}'");
        }

        if (document.Status == DocumentStatus.Deleted)
        {
            return Result<DocumentDownloadDto>.Fail("Document has been deleted");
        }

        // Verify requester exists (audit)
        var requester = await _tenantUow.Repository<Employee>().GetByIdAsync(req.RequestedById, ct);
        if (requester is null)
        {
            return Result<DocumentDownloadDto>.Fail($"Could not find employee with ID '{req.RequestedById}'");
        }

        try
        {
            var exists = await _blobStorage.ExistsAsync(document.BlobContainer, document.BlobPath, ct);
            if (!exists)
            {
                return Result<DocumentDownloadDto>.Fail("Document file not found in storage");
            }

            var stream = await _blobStorage.DownloadAsync(document.BlobContainer, document.BlobPath, ct);

            var dto = new DocumentDownloadDto
            {
                FileName = document.FileName,
                OriginalFileName = document.OriginalFileName,
                ContentType = document.ContentType,
                FileSizeBytes = document.FileSizeBytes,
                FileContent = stream
            };

            return Result<DocumentDownloadDto>.Succeed(dto);
        }
        catch (Exception ex)
        {
            return Result<DocumentDownloadDto>.Fail($"Failed to download document: {ex.Message}");
        }
    }
}
