using Logistics.Application.Abstractions;
using Logistics.Application.Constants;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class CaptureBillOfLadingHandler(
    ITenantUnitOfWork tenantUow,
    IBlobStorageService blobStorage,
    ILogger<CaptureBillOfLadingHandler> logger)
    : IAppRequestHandler<CaptureBillOfLadingCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CaptureBillOfLadingCommand req, CancellationToken ct)
    {
        // Verify load exists
        var load = await tenantUow.Repository<Load>().GetByIdAsync(req.LoadId, ct);
        if (load is null)
        {
            return Result<Guid>.Fail($"Load with ID '{req.LoadId}' not found");
        }

        // Verify employee exists
        var employee = await tenantUow.Repository<Employee>().GetByIdAsync(req.CapturedById, ct);
        if (employee is null)
        {
            return Result<Guid>.Fail($"Employee with ID '{req.CapturedById}' not found");
        }

        // Verify trip stop if provided
        if (req.TripStopId.HasValue)
        {
            var tripStop = await tenantUow.Repository<TripStop>().GetByIdAsync(req.TripStopId.Value, ct);
            if (tripStop is null)
            {
                return Result<Guid>.Fail($"Trip stop with ID '{req.TripStopId}' not found");
            }
        }

        var uploadedDocIds = new List<Guid>();
        var capturedAt = DateTime.UtcNow;
        string? signatureBlobPath = null;

        try
        {
            // Upload signature if provided
            if (!string.IsNullOrEmpty(req.SignatureBase64))
            {
                var signatureBytes = Convert.FromBase64String(req.SignatureBase64);
                var signatureFileName = $"{Guid.NewGuid()}_signature.png";
                signatureBlobPath = $"loads/{req.LoadId}/bol/{signatureFileName}";

                using var signatureStream = new MemoryStream(signatureBytes);
                await blobStorage.UploadAsync(
                    BlobConstants.DocumentsContainerName,
                    signatureBlobPath,
                    signatureStream,
                    "image/png",
                    ct);
            }

            // Upload photos
            foreach (var photo in req.Photos)
            {
                var ext = Path.GetExtension(photo.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}{ext}";
                var blobPath = $"loads/{req.LoadId}/bol/{uniqueFileName}";

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
                    DocumentType.BillOfLading,
                    req.LoadId,
                    req.CapturedById,
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

            // If no photos but we have signature/shipper info, create a single BOL record
            if (req.Photos.Count == 0 && (!string.IsNullOrEmpty(req.SignatureBase64) || !string.IsNullOrEmpty(req.RecipientName)))
            {
                var signatureFileName = $"{Guid.NewGuid()}_bol.json";
                var blobPath = $"loads/{req.LoadId}/bol/{signatureFileName}";

                // Create a placeholder document for the signature-only BOL
                var doc = DeliveryDocument.Create(
                    signatureFileName,
                    "bill_of_lading.json",
                    "application/json",
                    0,
                    blobPath,
                    BlobConstants.DocumentsContainerName,
                    DocumentType.BillOfLading,
                    req.LoadId,
                    req.CapturedById,
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
                    "BOL captured for load {LoadId}: {PhotoCount} photos, signature: {HasSignature}",
                    req.LoadId, req.Photos.Count, !string.IsNullOrEmpty(req.SignatureBase64));

                return Result<Guid>.Ok(uploadedDocIds.First());
            }

            return Result<Guid>.Fail("No documents were created");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to capture BOL for load {LoadId}", req.LoadId);

            // Cleanup any uploaded blobs on failure
            foreach (var docId in uploadedDocIds)
            {
                // Note: In production, you might want to track blob paths for cleanup
            }

            return Result<Guid>.Fail($"Failed to capture bill of lading: {ex.Message}");
        }
    }
}
