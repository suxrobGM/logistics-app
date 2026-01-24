using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class DownloadPublicTrackingDocumentHandler(
    ITenantUnitOfWork tenantUow,
    IBlobStorageService blobStorage)
    : IAppRequestHandler<DownloadPublicTrackingDocumentQuery, Result<DocumentDownloadDto>>
{
    public async Task<Result<DocumentDownloadDto>> Handle(
        DownloadPublicTrackingDocumentQuery req,
        CancellationToken ct)
    {
        // Set tenant context for the query
        try
        {
            await tenantUow.SetCurrentTenantByIdAsync(req.TenantId);
        }
        catch (InvalidOperationException)
        {
            return Result<DocumentDownloadDto>.Fail("Invalid tracking link.");
        }

        // Find the tracking link
        var trackingLink = await tenantUow.Repository<TrackingLink>()
            .GetAsync(t => t.Token == req.Token, ct);

        if (trackingLink is null)
        {
            return Result<DocumentDownloadDto>.Fail("Tracking link not found.");
        }

        if (!trackingLink.IsValid)
        {
            return Result<DocumentDownloadDto>.Fail("This tracking link has expired or been revoked.");
        }

        // Get the document and verify it belongs to the load
        var document = await tenantUow.Repository<LoadDocument>()
            .GetAsync(d => d.Id == req.DocumentId && d.LoadId == trackingLink.LoadId, ct);

        if (document is null)
        {
            return Result<DocumentDownloadDto>.Fail("Document not found.");
        }

        if (document.Status == DocumentStatus.Deleted)
        {
            return Result<DocumentDownloadDto>.Fail("Document has been deleted.");
        }

        try
        {
            var exists = await blobStorage.ExistsAsync(document.BlobContainer, document.BlobPath, ct);
            if (!exists)
            {
                return Result<DocumentDownloadDto>.Fail("Document file not found in storage.");
            }

            var stream = await blobStorage.DownloadAsync(document.BlobContainer, document.BlobPath, ct);

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
