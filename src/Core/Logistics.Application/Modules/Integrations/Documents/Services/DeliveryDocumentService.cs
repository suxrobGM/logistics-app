using Logistics.Application.Abstractions.Storage;
using Logistics.Application.Modules.Common.Constants;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Integrations.Documents.Services;

internal sealed class DeliveryDocumentService(
    ITenantUnitOfWork tenantUow,
    IBlobStorageService blobStorage,
    IDocumentAccessService documentAccess,
    ILogger<DeliveryDocumentService> logger) : IDeliveryDocumentService
{
    public async Task<Result<Guid>> CaptureAsync(
        DeliveryDocumentKind kind,
        CaptureDeliveryDocumentParameters parameters,
        CancellationToken ct = default)
    {
        var caller = await documentAccess.ResolveCallerAsync(ct);
        if (caller is null ||
            !await documentAccess.CanAccessOwnerAsync(caller, DocumentOwnerType.Load, parameters.LoadId, ct))
        {
            return Result<Guid>.Fail("Load not found or access denied.");
        }

        if (parameters.TripStopId.HasValue &&
            await tenantUow.Repository<TripStop>().GetByIdAsync(parameters.TripStopId.Value, ct) is null)
        {
            return Result<Guid>.Fail($"Trip stop with ID '{parameters.TripStopId}' not found");
        }

        var capture = new CaptureInProgress(kind, parameters, caller.CallerId);

        try
        {
            await UploadSignatureAsync(capture, ct);
            await UploadPhotosAsync(capture, ct);
            await AddSummaryDocumentIfNoPhotosAsync(capture, ct);

            var changes = await tenantUow.SaveChangesAsync(ct);
            if (changes > 0 && capture.DocumentIds.Count > 0)
            {
                logger.LogInformation(
                    "{Kind} captured for load {LoadId}: {PhotoCount} photos, signature: {HasSignature}",
                    kind.ShortName, parameters.LoadId, parameters.Photos.Count, capture.HasSignature);

                return Result<Guid>.Ok(capture.DocumentIds[0]);
            }

            await DiscardUploadsAsync(capture);
            return Result<Guid>.Fail("No documents were created");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to capture {Kind} for load {LoadId}", kind.ShortName, parameters.LoadId);
            await DiscardUploadsAsync(capture);

            return Result<Guid>.Fail(kind.FailureMessage);
        }
    }

    private async Task UploadSignatureAsync(CaptureInProgress capture, CancellationToken ct)
    {
        if (!capture.HasSignature)
        {
            return;
        }

        var blobPath = capture.TrackBlobPath(BlobPathHelper.GenerateSignatureFileName());
        capture.SignatureBlobPath = blobPath;

        using var signature = new MemoryStream(Convert.FromBase64String(capture.Parameters.SignatureBase64!));
        await blobStorage.UploadAsync(
            BlobConstants.DocumentsContainerName, blobPath, signature, "image/png", ct);
    }

    private async Task UploadPhotosAsync(CaptureInProgress capture, CancellationToken ct)
    {
        var photoIndex = 0;

        foreach (var photo in capture.Parameters.Photos)
        {
            var fileName = BlobPathHelper.GenerateUniqueFileName(photo.FileName, photoIndex++);
            var blobPath = capture.TrackBlobPath(fileName);

            await blobStorage.UploadAsync(
                BlobConstants.DocumentsContainerName, blobPath, photo.Content, photo.ContentType, ct);

            await AddDocumentAsync(
                capture, fileName, photo.FileName, photo.ContentType, photo.FileSizeBytes, blobPath, ct);
        }
    }

    private async Task AddSummaryDocumentIfNoPhotosAsync(CaptureInProgress capture, CancellationToken ct)
    {
        if (capture.Parameters.Photos.Count > 0 ||
            (!capture.HasSignature && string.IsNullOrEmpty(capture.Parameters.RecipientName)))
        {
            return;
        }

        var fileName = BlobPathHelper.GeneratePlaceholderFileName(capture.Kind.FolderName);
        var blobPath = BlobPathHelper.GetLoadBlobPath(
            capture.Parameters.LoadId, capture.Kind.FolderName, fileName);

        await AddDocumentAsync(capture, fileName, capture.Kind.SummaryFileName, "application/json", 0, blobPath, ct);
    }

    private async Task AddDocumentAsync(
        CaptureInProgress capture,
        string fileName,
        string originalFileName,
        string contentType,
        long fileSizeBytes,
        string blobPath,
        CancellationToken ct)
    {
        var parameters = capture.Parameters;

        var document = DeliveryDocument.Create(
            fileName,
            originalFileName,
            contentType,
            fileSizeBytes,
            blobPath,
            BlobConstants.DocumentsContainerName,
            capture.Kind.DocumentType,
            parameters.LoadId,
            capture.CallerId,
            parameters.RecipientName,
            capture.SignatureBlobPath,
            parameters.Latitude,
            parameters.Longitude,
            capture.CapturedAt,
            parameters.TripStopId,
            parameters.Notes);

        await tenantUow.Repository<DeliveryDocument>().AddAsync(document, ct);
        capture.DocumentIds.Add(document.Id);
    }

    private Task DiscardUploadsAsync(CaptureInProgress capture)
    {
        return DocumentBlobCleanup.DeleteAsync(blobStorage, capture.UploadedBlobPaths, logger);
    }

    /// <summary>Carries the blob paths written so far, so a failure can undo them.</summary>
    private sealed class CaptureInProgress(
        DeliveryDocumentKind kind,
        CaptureDeliveryDocumentParameters parameters,
        Guid callerId)
    {
        public DeliveryDocumentKind Kind { get; } = kind;
        public CaptureDeliveryDocumentParameters Parameters { get; } = parameters;
        public Guid CallerId { get; } = callerId;
        public DateTime CapturedAt { get; } = DateTime.UtcNow;
        public List<Guid> DocumentIds { get; } = [];
        public List<string> UploadedBlobPaths { get; } = [];
        public string? SignatureBlobPath { get; set; }

        public bool HasSignature => !string.IsNullOrEmpty(Parameters.SignatureBase64);

        public string TrackBlobPath(string fileName)
        {
            var blobPath = BlobPathHelper.GetLoadBlobPath(Parameters.LoadId, Kind.FolderName, fileName);
            UploadedBlobPaths.Add(blobPath);
            return blobPath;
        }
    }
}
