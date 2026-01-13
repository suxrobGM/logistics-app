using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Application.Services.Geocoding;
using Logistics.Application.Services.PdfImport;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class ImportLoadFromPdfHandler(
    IPdfDataExtractor pdfExtractor,
    IGeocodingService geocodingService,
    ILoadService loadService,
    ITenantUnitOfWork tenantUow,
    ILogger<ImportLoadFromPdfHandler> logger)
    : IAppRequestHandler<ImportLoadFromPdfCommand, Result<ImportLoadFromPdfResponse>>
{
    public async Task<Result<ImportLoadFromPdfResponse>> Handle(
        ImportLoadFromPdfCommand request,
        CancellationToken ct)
    {
        var warnings = new List<string>();

        // Step 1: Extract data from PDF
        var extractResult = await pdfExtractor.ExtractAsync(request.PdfContent, request.FileName, ct);

        if (!extractResult.Success)
        {
            return Result<ImportLoadFromPdfResponse>.Fail(extractResult.Error ?? "Failed to extract data from PDF");
        }

        var extractedData = extractResult.Data!;
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

            if (!originGeoResult.Success)
            {
                warnings.Add(
                    $"Origin address geocoding failed: {originGeoResult.Error}. Please add coordinates manually.");
            }
            else
            {
                originLocation = originGeoResult.Data;
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

            if (!destGeoResult.Success)
            {
                warnings.Add(
                    $"Destination address geocoding failed: {destGeoResult.Error}. Please add coordinates manually.");
            }
            else
            {
                destinationLocation = destGeoResult.Data;
            }
        }

        // Fail if we couldn't geocode either address
        if (originLocation is null || destinationLocation is null)
        {
            return Result<ImportLoadFromPdfResponse>.Fail(
                "Unable to geocode addresses. Please ensure the addresses are valid.");
        }

        // Step 4: Find or create customer
        var (customer, customerCreated) = await FindOrCreateCustomerAsync(extractedData.ShipperName, ct);

        if (customer is null)
        {
            return Result<ImportLoadFromPdfResponse>.Fail("Unable to find or create customer");
        }

        if (customerCreated)
        {
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

        // Step 6: Find truck (required)
        var truck = await tenantUow.Repository<Truck>()
            .GetAsync(t => t.Id == request.AssignedTruckId, ct);

        if (truck is null)
        {
            return Result<ImportLoadFromPdfResponse>.Fail("Specified truck not found.");
        }

        // Step 7: Create the load
        try
        {
            var createLoadParameters = new CreateLoadParameters(
                extractedData.GetLoadName(),
                LoadType.Vehicle,
                (extractedData.OriginAddress!, originLocation),
                (extractedData.DestinationAddress!, destinationLocation),
                extractedData.PaymentAmount ?? 0,
                0, // Will be calculated from coordinates
                customer.Id,
                truck.Id,
                dispatcher.Id);

            var newLoad = await loadService.CreateLoadAsync(createLoadParameters, ct: ct);

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
        catch (InvalidOperationException e)
        {
            logger.LogError(e, "Error creating load from PDF import");
            return Result<ImportLoadFromPdfResponse>.Fail(e.Message);
        }
    }

    private async Task<(Customer? customer, bool created)> FindOrCreateCustomerAsync(
        string? shipperName,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(shipperName))
        {
            // Use default customer name
            shipperName = "Imported Customer";
        }

        // Try to find existing customer by name
        var existingCustomer = await tenantUow.Repository<Customer>()
            .GetAsync(c => c.Name == shipperName, ct);

        if (existingCustomer is not null)
        {
            return (existingCustomer, false);
        }

        // Create new customer
        var newCustomer = new Customer
        {
            Name = shipperName
        };

        await tenantUow.Repository<Customer>().AddAsync(newCustomer, ct);
        await tenantUow.SaveChangesAsync(ct);

        return (newCustomer, true);
    }
}
