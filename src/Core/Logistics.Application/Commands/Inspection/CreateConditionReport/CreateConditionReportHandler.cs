using Logistics.Application.Abstractions;
using Logistics.Application.Constants;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class CreateConditionReportHandler(
    ITenantUnitOfWork tenantUow,
    IBlobStorageService blobStorage,
    ILogger<CreateConditionReportHandler> logger)
    : IAppRequestHandler<CreateConditionReportCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateConditionReportCommand req, CancellationToken ct)
    {
        // Verify load exists
        var load = await tenantUow.Repository<Load>().GetByIdAsync(req.LoadId, ct);
        if (load is null)
        {
            return Result<Guid>.Fail($"Load with ID '{req.LoadId}' not found");
        }

        // Verify inspector exists
        var inspector = await tenantUow.Repository<Employee>().GetByIdAsync(req.InspectedById, ct);
        if (inspector is null)
        {
            return Result<Guid>.Fail($"Employee with ID '{req.InspectedById}' not found");
        }

        string? signatureBlobPath = null;

        try
        {
            // Upload signature if provided
            if (!string.IsNullOrEmpty(req.SignatureBase64))
            {
                var signatureBytes = Convert.FromBase64String(req.SignatureBase64);
                var signatureFileName = $"{Guid.NewGuid()}_signature.png";
                signatureBlobPath = $"loads/{req.LoadId}/inspection/{signatureFileName}";

                using var signatureStream = new MemoryStream(signatureBytes);
                await blobStorage.UploadAsync(
                    BlobConstants.DocumentsContainerName,
                    signatureBlobPath,
                    signatureStream,
                    "image/png",
                    ct);
            }

            // Create condition report
            var report = VehicleConditionReport.Create(
                req.LoadId,
                req.Vin.ToUpperInvariant(),
                req.Type,
                req.InspectedById,
                req.DamageMarkersJson,
                req.Notes,
                req.Latitude,
                req.Longitude);

            report.VehicleYear = req.VehicleYear;
            report.VehicleMake = req.VehicleMake;
            report.VehicleModel = req.VehicleModel;
            report.VehicleBodyClass = req.VehicleBodyClass;
            report.InspectorSignature = signatureBlobPath;

            await tenantUow.Repository<VehicleConditionReport>().AddAsync(report, ct);

            // Upload and attach photos
            foreach (var photo in req.Photos)
            {
                var ext = Path.GetExtension(photo.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}{ext}";
                var blobPath = $"loads/{req.LoadId}/inspection/{uniqueFileName}";

                await blobStorage.UploadAsync(
                    BlobConstants.DocumentsContainerName,
                    blobPath,
                    photo.Content,
                    photo.ContentType,
                    ct);

                var docType = req.Type == InspectionType.Pickup
                    ? DocumentType.PickupInspection
                    : DocumentType.DeliveryInspection;

                var doc = DeliveryDocument.Create(
                    uniqueFileName,
                    photo.FileName,
                    photo.ContentType,
                    photo.FileSizeBytes,
                    blobPath,
                    BlobConstants.DocumentsContainerName,
                    docType,
                    req.LoadId,
                    req.InspectedById,
                    recipientName: null,
                    recipientSignature: null,
                    captureLatitude: req.Latitude,
                    captureLongitude: req.Longitude,
                    capturedAt: report.InspectedAt,
                    tripStopId: null,
                    notes: $"Vehicle inspection - {req.Vin}");

                await tenantUow.Repository<DeliveryDocument>().AddAsync(doc, ct);
            }

            await tenantUow.SaveChangesAsync(ct);

            logger.LogInformation(
                "Condition report created for load {LoadId}, VIN: {Vin}, Type: {Type}",
                req.LoadId, req.Vin, req.Type);

            return Result<Guid>.Ok(report.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create condition report for load {LoadId}", req.LoadId);
            return Result<Guid>.Fail($"Failed to create condition report: {ex.Message}");
        }
    }
}
