using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Common.Constants;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.Storage;

namespace Logistics.Application.Modules.Integrations.Documents.Commands;

internal sealed class CaptureProofOfDeliveryHandler(
    ITenantUnitOfWork tenantUow,
    IBlobStorageService blobStorage,
    ICurrentUserService currentUserService,
    ILogger<CaptureProofOfDeliveryHandler> logger)
    : IAppRequestHandler<CaptureProofOfDeliveryCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CaptureProofOfDeliveryCommand req, CancellationToken ct)
    {
        var access = await DocumentAccess.ResolveAsync(tenantUow, currentUserService, ct);
        if (access is null ||
            !await DocumentAccess.CanAccessOwnerAsync(
                tenantUow, access, DocumentOwnerType.Load, req.LoadId, ct))
        {
            return Result<Guid>.Fail("Load not found or access denied.");
        }

        if (req.TripStopId.HasValue)
        {
            var tripStop = await tenantUow.Repository<TripStop>().GetByIdAsync(req.TripStopId.Value, ct);
            if (tripStop is null)
            {
                return Result<Guid>.Fail($"Trip stop with ID '{req.TripStopId}' not found");
            }
        }

        var uploadedDocIds = new List<Guid>();
        var uploadedBlobPaths = new List<string>();
        var capturedAt = DateTime.UtcNow;
        string? signatureBlobPath = null;

        try
        {
            if (!string.IsNullOrEmpty(req.SignatureBase64))
            {
                var signatureBytes = Convert.FromBase64String(req.SignatureBase64);
                var signatureFileName = BlobPathHelper.GenerateSignatureFileName();
                signatureBlobPath = BlobPathHelper.GetLoadBlobPath(req.LoadId, "pod", signatureFileName);
                uploadedBlobPaths.Add(signatureBlobPath);

                using var signatureStream = new MemoryStream(signatureBytes);
                await blobStorage.UploadAsync(
                    BlobConstants.DocumentsContainerName,
                    signatureBlobPath,
                    signatureStream,
                    "image/png",
                    ct);
            }

            var photoIndex = 0;
            foreach (var photo in req.Photos)
            {
                var uniqueFileName = BlobPathHelper.GenerateUniqueFileName(photo.FileName, photoIndex++);
                var blobPath = BlobPathHelper.GetLoadBlobPath(req.LoadId, "pod", uniqueFileName);
                uploadedBlobPaths.Add(blobPath);

                await blobStorage.UploadAsync(
                    BlobConstants.DocumentsContainerName,
                    blobPath,
                    photo.Content,
                    photo.ContentType,
                    ct);

                var doc = DeliveryDocument.Create(
                    uniqueFileName,
                    photo.FileName,
                    photo.ContentType,
                    photo.FileSizeBytes,
                    blobPath,
                    BlobConstants.DocumentsContainerName,
                    DocumentType.ProofOfDelivery,
                    req.LoadId,
                    access.CallerId,
                    req.RecipientName,
                    signatureBlobPath,
                    req.Latitude,
                    req.Longitude,
                    capturedAt,
                    req.TripStopId,
                    req.Notes);

                await tenantUow.Repository<DeliveryDocument>().AddAsync(doc, ct);
                uploadedDocIds.Add(doc.Id);
            }

            if (req.Photos.Count == 0 && (!string.IsNullOrEmpty(req.SignatureBase64) || !string.IsNullOrEmpty(req.RecipientName)))
            {
                var podFileName = BlobPathHelper.GeneratePlaceholderFileName("pod");
                var blobPath = BlobPathHelper.GetLoadBlobPath(req.LoadId, "pod", podFileName);

                var doc = DeliveryDocument.Create(
                    podFileName,
                    "proof_of_delivery.json",
                    "application/json",
                    0,
                    blobPath,
                    BlobConstants.DocumentsContainerName,
                    DocumentType.ProofOfDelivery,
                    req.LoadId,
                    access.CallerId,
                    req.RecipientName,
                    signatureBlobPath,
                    req.Latitude,
                    req.Longitude,
                    capturedAt,
                    req.TripStopId,
                    req.Notes);

                await tenantUow.Repository<DeliveryDocument>().AddAsync(doc, ct);
                uploadedDocIds.Add(doc.Id);
            }

            var changes = await tenantUow.SaveChangesAsync(ct);
            if (changes > 0 && uploadedDocIds.Count > 0)
            {
                logger.LogInformation(
                    "POD captured for load {LoadId}: {PhotoCount} photos, signature: {HasSignature}",
                    req.LoadId, req.Photos.Count, !string.IsNullOrEmpty(req.SignatureBase64));

                return Result<Guid>.Ok(uploadedDocIds.First());
            }

            await DocumentBlobCleanup.DeleteAsync(blobStorage, uploadedBlobPaths, logger);
            return Result<Guid>.Fail("No documents were created");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to capture POD for load {LoadId}", req.LoadId);
            await DocumentBlobCleanup.DeleteAsync(blobStorage, uploadedBlobPaths, logger);

            return Result<Guid>.Fail("Failed to capture proof of delivery.");
        }
    }
}
