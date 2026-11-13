using Logistics.Application.Modules.Operations.Loads.Services;
using Logistics.Application.Modules.IdentityAccess.Customers.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.Geocoding;
using Logistics.Application.Abstractions.Documents;

namespace Logistics.Application.Modules.Operations.Loads.Commands;

internal sealed class ImportLoadFromPdfHandler(
    IPdfDataExtractor pdfExtractor,
    IGeocodingService geocodingService,
    ILoadService loadService,
    ITenantUnitOfWork tenantUow,
    ILogger<ImportLoadFromPdfHandler> logger)
    : IRequestHandler<ImportLoadFromPdfCommand, Result<ImportLoadFromPdfResponse>>
{
    public async Task<Result<ImportLoadFromPdfResponse>> Handle(
        ImportLoadFromPdfCommand request,
        CancellationToken ct)
    {
        var warnings = new List<string>();

        // Step 1: Extract data from PDF
        var extractResult = await pdfExtractor.ExtractAsync(request.PdfContent, request.FileName, ct);

        if (!extractResult.IsSuccess)
        {
            return Result<ImportLoadFromPdfResponse>.Fail(extractResult.Error ?? "Failed to extract data from PDF");
        }

        var extractedData = extractResult.Value!;
        logger.LogInformation("Extracted data from PDF {FileName} using template {Template}",
            request.FileName, extractedData.SourceTemplate);

        // Step 2: Geocode origin address
        GeoPoint? originLocation = null;
        if (extractedData.OriginAddress is not null)
        {
            var originGeoResult = await geocodingService.GeocodeAddressAsync(
                extractedData.OriginAddress.Line1 ?? "",
                extractedData.OriginAddress.City ?? "",
                extractedData.OriginAddress.State ?? "",
                extractedData.OriginAddress.ZipCode,
                extractedData.OriginAddress.Country,
                ct);

            if (!originGeoResult.IsSuccess)
            {
                warnings.Add(
                    $"Origin address geocoding failed: {originGeoResult.Error}. Please add coordinates manually.");
            }
            else
            {
                originLocation = originGeoResult.Value;
            }
        }

        // Step 3: Geocode destination address
        GeoPoint? destinationLocation = null;
        if (extractedData.DestinationAddress is not null)
        {
            var destGeoResult = await geocodingService.GeocodeAddressAsync(
                extractedData.DestinationAddress.Line1 ?? "",
                extractedData.DestinationAddress.City ?? "",
                extractedData.DestinationAddress.State ?? "",
                extractedData.DestinationAddress.ZipCode,
                extractedData.DestinationAddress.Country,
                ct);

            if (!destGeoResult.IsSuccess)
            {
                warnings.Add(
                    $"Destination address geocoding failed: {destGeoResult.Error}. Please add coordinates manually.");
            }
            else
            {
                destinationLocation = destGeoResult.Value;
            }
        }

        // Fail if we couldn't geocode either address
        if (originLocation is null || destinationLocation is null)
        {
            return Result<ImportLoadFromPdfResponse>.Fail(
                "Unable to geocode addresses. Please ensure the addresses are valid.");
        }

        // Step 4: Find or create customer
        var shipperName = string.IsNullOrEmpty(extractedData.ShipperName) ? "Imported Customer" : extractedData.ShipperName;
        var (customer, customerCreated) = await tenantUow.FindOrAddCustomerAsync(shipperName, ct);
        if (customerCreated)
        {
            // LoadService reads the customer back from the database, so it has to exist first.
            await tenantUow.SaveChangesAsync(ct);
            logger.LogInformation("Created new customer: {CustomerName}", customer.Name);
        }

        // Step 5: Find dispatcher (current user must be an employee)
        var dispatcher = await tenantUow.Repository<Employee>()
            .GetAsync(e => e.Id == request.CurrentUserId, ct);

        if (dispatcher is null)
        {
            return Result<ImportLoadFromPdfResponse>.Fail(
                "Current user is not an employee. Cannot assign as dispatcher.");
        }

        // Step 6: Find truck (optional)
        Guid? truckId = null;
        if (request.AssignedTruckId.HasValue && request.AssignedTruckId.Value != Guid.Empty)
        {
            var truck = await tenantUow.Repository<Truck>()
                .GetAsync(t => t.Id == request.AssignedTruckId.Value, ct);

            if (truck is null)
            {
                return Result<ImportLoadFromPdfResponse>.Fail("Specified truck not found.", ErrorCodes.NotFound);
            }

            truckId = truck.Id;
        }

        // Step 7: Create the load
        var details = new CreateLoadCommand
        {
            Name = extractedData.GetLoadName(),
            Type = LoadType.Vehicle,
            OriginAddress = extractedData.OriginAddress!,
            OriginLocation = originLocation,
            DestinationAddress = extractedData.DestinationAddress!,
            DestinationLocation = destinationLocation,
            DeliveryCost = extractedData.PaymentAmount ?? 0,
            CustomerId = customer.Id,
            AssignedDispatcherId = dispatcher.Id,
            Source = LoadSource.Pdf,
            RequestedPickupDate = extractedData.PickupDate,
            RequestedDeliveryDate = extractedData.DeliveryDate,
            PickupAppointmentFrom = extractedData.PickupAppointmentFrom,
            PickupAppointmentTo = extractedData.PickupAppointmentTo,
            DeliveryAppointmentFrom = extractedData.DeliveryAppointmentFrom,
            DeliveryAppointmentTo = extractedData.DeliveryAppointmentTo,
            Vehicles = extractedData.Vehicles
        };

        var created = await loadService.CreateLoadAsync(new NewLoad(details, truckId), ct: ct);
        if (!created.IsSuccess)
        {
            logger.LogError("Error creating load from PDF import: {Error}", created.Error);
            return Result<ImportLoadFromPdfResponse>.Fail(created);
        }

        var newLoad = created.Value!;
        logger.LogInformation("Created load {LoadId} from PDF import", newLoad.Id);

        return Result<ImportLoadFromPdfResponse>.Ok(new ImportLoadFromPdfResponse
        {
            LoadId = newLoad.Id,
            LoadName = newLoad.Name,
            LoadNumber = newLoad.Number,
            ExtractedData = extractedData,
            CustomerCreated = customerCreated,
            CustomerName = customer.Name,
            Warnings = warnings
        });
    }
}
