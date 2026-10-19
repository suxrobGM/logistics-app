using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Abstractions.Documents;
using Logistics.Application.Abstractions.Storage;
using Logistics.Application.Modules.Common.Constants;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Operations.Loads.Queries;

internal sealed class GetVehicleBolPdfHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUserService,
    IBlobStorageService blobStorage,
    IVehicleBolPdfService pdfService,
    ILogger<GetVehicleBolPdfHandler> logger)
    : IRequestHandler<GetVehicleBolPdfQuery, Result<VehicleBolPdfResult>>
{
    public async Task<Result<VehicleBolPdfResult>> Handle(GetVehicleBolPdfQuery req, CancellationToken ct)
    {
        var load = await tenantUow.Repository<Load>().GetByIdAsync(req.LoadId, ct);

        var canRead = load is not null && (req.CustomerId is { } customerId
            ? load.CustomerId == customerId
            : currentUserService.CanDrive(load.AssignedTruck));

        if (!canRead)
        {
            return Result<VehicleBolPdfResult>.Fail($"Could not find a load with ID '{req.LoadId}'");
        }

        if (load!.Type != LoadType.Vehicle || load.Vehicles.Count == 0)
        {
            return Result<VehicleBolPdfResult>.Fail("An electronic BOL is only available for vehicle loads that list their vehicles.");
        }

        var reports = await tenantUow.Repository<LoadConditionReport>().Query()
            .Where(r => r.LoadId == load.Id && r.LoadVehicleId != null)
            .OrderByDescending(r => r.InspectedAt)
            .ToListAsync(ct);

        var vehicles = load.Vehicles
            .Select(v => new VehicleBolVehicle(
                v,
                ToInspection(reports.FirstOrDefault(r => r.LoadVehicleId == v.Id && r.Type == InspectionType.Pickup)),
                ToInspection(reports.FirstOrDefault(r => r.LoadVehicleId == v.Id && r.Type == InspectionType.Delivery))))
            .ToList();

        var document = new VehicleBolDocument(
            tenantUow.GetCurrentTenant(),
            load.Number,
            load.Name,
            load.Customer.Name,
            load.OriginAddress,
            load.DestinationAddress,
            load.AssignedTruck?.Number,
            load.AssignedTruck?.MainDriver?.GetFullName(),
            load.DeliveryCost.Currency,
            vehicles,
            await GetSignatureAsync(load.Id, DocumentType.BillOfLading, ct),
            await GetSignatureAsync(load.Id, DocumentType.ProofOfDelivery, ct),
            DateTime.UtcNow);

        return Result<VehicleBolPdfResult>.Ok(
            new VehicleBolPdfResult(pdfService.Generate(document), $"eBOL-{load.Number}.pdf"));
    }

    private static VehicleBolInspection? ToInspection(LoadConditionReport? report) =>
        report is null
            ? null
            : new VehicleBolInspection(
                report.InspectedAt,
                report.InspectedBy?.GetFullName(),
                report.Latitude,
                report.Longitude,
                report.Notes,
                report.Defects);

    /// <summary>
    /// The latest BOL (shipper, at pickup) or POD (receiver, at delivery) capture that carries a
    /// signer, a signature or a reason nobody signed.
    /// </summary>
    private async Task<VehicleBolSignature?> GetSignatureAsync(Guid loadId, DocumentType type, CancellationToken ct)
    {
        var capture = await tenantUow.Repository<DeliveryDocument>().Query()
            .Where(d => d.LoadId == loadId
                        && d.Type == type
                        && (d.RecipientSignature != null || d.RecipientName != null || d.UnableToSignReason != null))
            .OrderByDescending(d => d.CapturedAt)
            .FirstOrDefaultAsync(ct);

        if (capture is null)
        {
            return null;
        }

        return new VehicleBolSignature(
            capture.RecipientName,
            await DownloadSignatureAsync(capture.RecipientSignature, ct),
            capture.UnableToSignReason,
            capture.CapturedAt,
            capture.CaptureLatitude,
            capture.CaptureLongitude);
    }

    private async Task<byte[]?> DownloadSignatureAsync(string? blobPath, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(blobPath))
        {
            return null;
        }

        try
        {
            return await blobStorage.DownloadBytesAsync(BlobConstants.DocumentsContainerName, blobPath, ct);
        }
        catch (Exception ex)
        {
            // A missing signature image still leaves the signer's name and time on the BOL.
            logger.LogWarning(ex, "Could not read signature {BlobPath} for the electronic BOL", blobPath);
            return null;
        }
    }
}
