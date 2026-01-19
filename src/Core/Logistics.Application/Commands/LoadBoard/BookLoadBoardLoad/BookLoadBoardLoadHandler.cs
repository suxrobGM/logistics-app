using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class BookLoadBoardLoadHandler(
    ITenantUnitOfWork tenantUow,
    ILoadBoardProviderFactory providerFactory,
    ILogger<BookLoadBoardLoadHandler> logger)
    : IAppRequestHandler<BookLoadBoardLoadCommand, Result<LoadBoardBookingResultDto>>
{
    public async Task<Result<LoadBoardBookingResultDto>> Handle(
        BookLoadBoardLoadCommand req,
        CancellationToken ct)
    {
        // Get the listing
        var listing = await tenantUow.Repository<LoadBoardListing>().GetByIdAsync(req.ListingId, ct);
        if (listing is null)
        {
            return Result<LoadBoardBookingResultDto>.Fail("Load board listing not found");
        }

        if (listing.Status != LoadBoardListingStatus.Available)
        {
            return Result<LoadBoardBookingResultDto>.Fail(
                $"Load board listing is not available (current status: {listing.Status})");
        }

        // Get provider configuration
        var providerConfig = await tenantUow.Repository<LoadBoardConfiguration>()
            .GetAsync(c => c.ProviderType == listing.ProviderType && c.IsActive, ct);

        if (providerConfig is null)
        {
            return Result<LoadBoardBookingResultDto>.Fail(
                $"No active provider configuration found for {listing.ProviderType}");
        }

        // Get truck
        var truck = await tenantUow.Repository<Truck>().GetByIdAsync(req.TruckId, ct);
        if (truck is null)
        {
            return Result<LoadBoardBookingResultDto>.Fail("Truck not found");
        }

        // Get dispatcher
        var dispatcher = await tenantUow.Repository<Employee>().GetByIdAsync(req.DispatcherId, ct);
        if (dispatcher is null)
        {
            return Result<LoadBoardBookingResultDto>.Fail("Dispatcher not found");
        }

        // Get or create customer
        Customer? customer;
        if (req.CustomerId.HasValue)
        {
            customer = await tenantUow.Repository<Customer>().GetByIdAsync(req.CustomerId.Value, ct);
            if (customer is null)
            {
                return Result<LoadBoardBookingResultDto>.Fail("Customer not found");
            }
        }
        else
        {
            // Create customer from broker info
            var customerName = req.CustomerName ?? listing.BrokerName ?? "Unknown Broker";
            customer = await tenantUow.Repository<Customer>()
                .GetAsync(c => c.Name == customerName, ct);

            if (customer is null)
            {
                customer = new Customer
                {
                    Name = customerName
                };
                await tenantUow.Repository<Customer>().AddAsync(customer, ct);
            }
        }

        // Book the load with the provider
        var provider = providerFactory.GetProvider(providerConfig);
        var bookingResult = await provider.BookLoadAsync(listing.ExternalListingId, new LoadBoardBookingRequest
        {
            TruckId = req.TruckId,
            DispatcherId = req.DispatcherId,
            CustomerId = req.CustomerId,
            CustomerName = req.CustomerName,
            Notes = req.Notes
        });

        if (!bookingResult.Success)
        {
            return Result<LoadBoardBookingResultDto>.Fail(
                bookingResult.ErrorMessage ?? "Failed to book load with provider");
        }

        // Determine load type based on equipment type
        var loadType = listing.EquipmentType?.ToLowerInvariant() switch
        {
            "flatbed" => LoadType.GeneralFreight,
            "dry van" => LoadType.GeneralFreight,
            "reefer" => LoadType.RefrigeratedGoods,
            "car carrier" or "auto carrier" or "car hauler" => LoadType.Vehicle,
            "tanker" => LoadType.Liquid,
            _ => LoadType.GeneralFreight
        };

        // Create the TMS Load
        var load = Load.Create(
            name: $"Load Board - {listing.BrokerName ?? listing.ProviderType.ToString()}",
            type: loadType,
            deliveryCost: listing.TotalRate?.Amount ?? 0,
            originAddress: listing.OriginAddress,
            originLocation: listing.OriginLocation,
            destinationAddress: listing.DestinationAddress,
            destinationLocation: listing.DestinationLocation,
            customer: customer,
            assignedTruck: truck,
            assignedDispatcher: dispatcher
        );

        // Set external source info
        load.ExternalSourceProvider = listing.ProviderType;
        load.ExternalSourceId = listing.ExternalListingId;
        load.ExternalBrokerReference = bookingResult.ExternalConfirmationId;

        await tenantUow.Repository<Load>().AddAsync(load, ct);

        // Update the listing
        listing.Status = LoadBoardListingStatus.Booked;
        listing.BookedAt = DateTime.UtcNow;
        listing.LoadId = load.Id;
        listing.Notes = req.Notes;

        await tenantUow.SaveChangesAsync(ct);

        logger.LogInformation(
            "Booked load board listing {ListingId} from {Provider}, created load {LoadId}",
            listing.Id, listing.ProviderType, load.Id);

        return Result<LoadBoardBookingResultDto>.Ok(new LoadBoardBookingResultDto
        {
            Success = true,
            ExternalConfirmationId = bookingResult.ExternalConfirmationId,
            CreatedLoadId = load.Id,
            CreatedLoadNumber = load.Number
        });
    }
}
