using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Storage;
using Logistics.Application.Modules.Common.Constants;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Compliance.Inspections.Commands;

internal sealed class CreateConditionReportHandler(
    ITenantUnitOfWork tenantUow,
    IBlobStorageService blobStorage,
    ILogger<CreateConditionReportHandler> logger)
    : IRequestHandler<CreateConditionReportCommand, Result<Guid>>
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

        var vehicleCheck = ResolveVehicle(load, req.LoadVehicleId);
        if (!vehicleCheck.IsSuccess)
        {
            return Result<Guid>.Fail(vehicleCheck.Error!);
        }

        var defectError = ValidateDefects(req, load.Type);
        if (defectError is not null)
        {
            return Result<Guid>.Fail(defectError);
        }

        var vehicle = vehicleCheck.Value;
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

            // Identifier fields - populated only for the matching cargo type
            if (load.Type == LoadType.Vehicle)
            {
                report.LoadVehicleId = vehicle?.Id;
                var vin = string.IsNullOrWhiteSpace(req.Vin) ? vehicle?.Vin : req.Vin;
                report.Vin = VinFormat.Normalize(vin);
                report.VehicleYear = req.VehicleYear ?? vehicle?.Year;
                report.VehicleMake = req.VehicleMake ?? vehicle?.Make;
                report.VehicleModel = req.VehicleModel ?? vehicle?.Model;
                report.VehicleBodyClass = req.VehicleBodyClass;
            }
            else if (load.Type.IsContainerLoad())
            {
                // Canonical like Container.Number, but stays null when absent (Normalize gives "").
                report.ContainerNumber = string.IsNullOrWhiteSpace(req.ContainerNumber)
                    ? null
                    : Container.NormalizeNumber(req.ContainerNumber);
                report.SealNumber = req.SealNumber;
            }

            report.InspectorSignature = signatureBlobPath;

            await tenantUow.Repository<LoadConditionReport>().AddAsync(report, ct);

            var photoIds = await AddPhotosAsync(req, load, report, vehicle, ct);

            foreach (var input in req.Defects)
            {
                var defect = new ConditionDefect
                {
                    PartCategory = input.ResolvePartCategory()!.Value,
                    Description = input.Description?.Trim() ?? string.Empty,
                    Severity = input.Severity,
                    Zone = input.Zone,
                    DamageType = input.DamageType,
                    PhotoDocumentId = input.PhotoIndex is { } index ? photoIds[index] : null
                };

                report.Defects.Add(defect);
                await tenantUow.Repository<ConditionDefect>().AddAsync(defect, ct);
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

    /// <summary>
    /// A vehicle load that lists its vehicles is inspected one vehicle at a time.
    /// </summary>
    private static Result<LoadVehicle?> ResolveVehicle(Load load, Guid? loadVehicleId)
    {
        if (loadVehicleId is null)
        {
            return load.Type == LoadType.Vehicle && load.Vehicles.Count > 0
                ? Result<LoadVehicle?>.Fail("Choose which of the load's vehicles this report inspects.")
                : Result<LoadVehicle?>.Ok(null);
        }

        var vehicle = load.FindVehicle(loadVehicleId.Value);
        return vehicle is null
            ? Result<LoadVehicle?>.Fail($"Vehicle '{loadVehicleId}' is not on load #{load.Number}.")
            : Result<LoadVehicle?>.Ok(vehicle);
    }

    private static string? ValidateDefects(CreateConditionReportCommand req, LoadType loadType)
    {
        var catalog = CargoInspectionPartCategoryExtensions.GetCatalogFor(loadType);

        foreach (var defect in req.Defects)
        {
            if (defect.ResolvePartCategory() is not { } category)
            {
                return "Each defect needs a part category or a vehicle diagram zone.";
            }

            if (!catalog.Contains(category))
            {
                return $"Defect category '{category}' is not valid for load type '{loadType}'";
            }

            if (defect.DamageType is null && string.IsNullOrWhiteSpace(defect.Description))
            {
                return "Each defect needs a damage type or a description.";
            }

            if (defect.PhotoIndex is { } index && (index < 0 || index >= req.Photos.Count))
            {
                return $"Defect photo index {index} does not match an uploaded photo.";
            }
        }

        return null;
    }

    private async Task<List<Guid>> AddPhotosAsync(
        CreateConditionReportCommand req,
        Load load,
        LoadConditionReport report,
        LoadVehicle? vehicle,
        CancellationToken ct)
    {
        var docType = req.Type == InspectionType.Pickup
            ? DocumentType.PickupInspection
            : DocumentType.DeliveryInspection;

        var inspectionLabel = load.Type == LoadType.Vehicle
            ? vehicle?.GetDisplayName() ?? report.Vin ?? "VIN n/a"
            : load.Type.IsContainerLoad()
                ? report.ContainerNumber ?? "Container n/a"
                : $"Load #{load.Number}";

        var photoIds = new List<Guid>(req.Photos.Count);
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

            report.Photos.Add(doc);
            await tenantUow.Repository<DeliveryDocument>().AddAsync(doc, ct);
            photoIds.Add(doc.Id);
        }

        return photoIds;
    }
}
