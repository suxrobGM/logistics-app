using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Storage;
using Logistics.Application.Modules.Common.Constants;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Compliance.Inspections.Commands;

internal sealed class CreateConditionReportHandler(
    ITenantUnitOfWork tenantUow,
    IBlobStorageService blobStorage,
    ILogger<CreateConditionReportHandler> logger)
    : IAppRequestHandler<CreateConditionReportCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateConditionReportCommand req, CancellationToken ct)
    {
        var load = await tenantUow.Repository<Load>().GetByIdAsync(req.LoadId, ct);
        if (load is null)
        {
            return Result<Guid>.Fail($"Load with ID '{req.LoadId}' not found");
        }

        var inspector = await tenantUow.Repository<Employee>().GetByIdAsync(req.InspectedById, ct);
        if (inspector is null)
        {
            return Result<Guid>.Fail($"Employee with ID '{req.InspectedById}' not found");
        }

        // Validate defect categories against the load's cargo type catalog
        var catalog = CargoInspectionPartCategoryExtensions.GetCatalogFor(load.Type);
        foreach (var defect in req.Defects)
        {
            if (!catalog.Contains(defect.PartCategory))
            {
                return Result<Guid>.Fail(
                    $"Defect category '{defect.PartCategory}' is not valid for load type '{load.Type}'");
            }
        }

        string? signatureBlobPath = null;

        try
        {
            if (!string.IsNullOrEmpty(req.SignatureBase64))
            {
                var signatureBytes = Convert.FromBase64String(req.SignatureBase64);
                var signatureFileName = BlobPathHelper.GenerateSignatureFileName();
                signatureBlobPath = BlobPathHelper.GetLoadBlobPath(req.LoadId, "inspection", signatureFileName);

                using var signatureStream = new MemoryStream(signatureBytes);
                await blobStorage.UploadAsync(
                    BlobConstants.DocumentsContainerName,
                    signatureBlobPath,
                    signatureStream,
                    "image/png",
                    ct);
            }

            var report = LoadConditionReport.Create(
                req.LoadId,
                req.Type,
                req.InspectedById,
                req.Notes,
                req.Latitude,
                req.Longitude);

            // Identifier fields — populated only for the matching cargo type
            if (load.Type == LoadType.Vehicle && !string.IsNullOrWhiteSpace(req.Vin))
            {
                report.Vin = req.Vin.ToUpperInvariant();
                report.VehicleYear = req.VehicleYear;
                report.VehicleMake = req.VehicleMake;
                report.VehicleModel = req.VehicleModel;
                report.VehicleBodyClass = req.VehicleBodyClass;
            }
            else if (load.Type.IsContainerLoad())
            {
                report.ContainerNumber = req.ContainerNumber?.ToUpperInvariant();
                report.SealNumber = req.SealNumber;
            }

            report.InspectorSignature = signatureBlobPath;

            foreach (var input in req.Defects)
            {
                report.Defects.Add(new ConditionDefect
                {
                    PartCategory = input.PartCategory,
                    Description = input.Description,
                    Severity = input.Severity
                });
            }

            await tenantUow.Repository<LoadConditionReport>().AddAsync(report, ct);

            var photoIndex = 0;
            foreach (var photo in req.Photos)
            {
                var uniqueFileName = BlobPathHelper.GenerateUniqueFileName(photo.FileName, photoIndex++);
                var blobPath = BlobPathHelper.GetLoadBlobPath(req.LoadId, "inspection", uniqueFileName);

                await blobStorage.UploadAsync(
                    BlobConstants.DocumentsContainerName,
                    blobPath,
                    photo.Content,
                    photo.ContentType,
                    ct);

                var docType = req.Type == InspectionType.Pickup
                    ? DocumentType.PickupInspection
                    : DocumentType.DeliveryInspection;

                var inspectionLabel = load.Type == LoadType.Vehicle
                    ? report.Vin ?? "VIN n/a"
                    : load.Type.IsContainerLoad()
                        ? report.ContainerNumber ?? "Container n/a"
                        : $"Load #{load.Number}";

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
                    notes: $"Cargo inspection - {inspectionLabel}");

                await tenantUow.Repository<DeliveryDocument>().AddAsync(doc, ct);
            }

            await tenantUow.SaveChangesAsync(ct);

            logger.LogInformation(
                "Condition report {ReportId} created for load {LoadId} (type {LoadType}, inspection {InspectionType})",
                report.Id, req.LoadId, load.Type, req.Type);

            return Result<Guid>.Ok(report.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create condition report for load {LoadId}", req.LoadId);
            return Result<Guid>.Fail($"Failed to create condition report: {ex.Message}");
        }
    }
}
