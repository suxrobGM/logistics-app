using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.LoadBoard;
using Logistics.Application.Modules.Integrations.LoadBoard.Services;
using Logistics.Application.Modules.Integrations.Negotiation.Services;
using Logistics.Mappings;

namespace Logistics.Application.Modules.Integrations.LoadBoard.Commands;

internal sealed class BookLoadBoardLoadHandler(
    ITenantUnitOfWork tenantUow,
    ILoadBoardTokenService tokenService,
    IBrokerCreditService brokerCreditService,
    IInboundEmailRouteRegistry routeRegistry,
    IAIDispatchBroadcastService broadcastService,
    ILogger<BookLoadBoardLoadHandler> logger)
    : IAppRequestHandler<BookLoadBoardLoadCommand, Result<LoadBoardBookingResultDto>>
{
    public async Task<Result<LoadBoardBookingResultDto>> Handle(
        BookLoadBoardLoadCommand req,
        CancellationToken ct)
    {
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

        var providerConfig = await tenantUow.Repository<LoadBoardConfiguration>()
            .GetAsync(c => c.ProviderType == listing.ProviderType && c.IsActive, ct);

        if (providerConfig is null)
        {
            return Result<LoadBoardBookingResultDto>.Fail(
                $"No active provider configuration found for {listing.ProviderType}");
        }

        // Resolve before staging entities: a token acquisition saves the scope mid-handler
        var providerResult = await tokenService.GetReadyProviderAsync(providerConfig, ct);
        if (!providerResult.IsSuccess || providerResult.Value is null)
        {
            return Result<LoadBoardBookingResultDto>.Fail(
                providerResult.Error ?? $"Could not authenticate with {listing.ProviderType}");
        }

        var provider = providerResult.Value;

        var truck = await tenantUow.Repository<Truck>().GetByIdAsync(req.TruckId, ct);
        if (truck is null)
        {
            return Result<LoadBoardBookingResultDto>.Fail("Truck not found");
        }

        var dispatcher = await tenantUow.Repository<Employee>().GetByIdAsync(req.DispatcherId, ct);
        if (dispatcher is null)
        {
            return Result<LoadBoardBookingResultDto>.Fail("Dispatcher not found");
        }

        var creditGate = await BrokerCreditGate.EvaluateAsync(
            tenantUow, brokerCreditService, listing, req.OverrideCreditCheck, ct);

        if (!creditGate.IsSuccess)
        {
            return Result<LoadBoardBookingResultDto>.Fail(creditGate.Error!, creditGate.ErrorCode!);
        }

        var negotiation = await tenantUow.Repository<RateNegotiation>()
            .GetAsync(RateNegotiation.OpenForListing(listing.Id), ct);

        var bookedRate = req.NegotiatedTotalRate ?? listing.TotalRate?.Amount ?? 0;

        var bookingRateCheck = CheckBookingRate(req.NegotiatedTotalRate, bookedRate, negotiation);
        if (!bookingRateCheck.IsSuccess)
        {
            return Result<LoadBoardBookingResultDto>.Fail(
                bookingRateCheck.Error!, bookingRateCheck.ErrorCode!);
        }

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

        var loadType = listing.EquipmentType?.ToLowerInvariant() switch
        {
            "flatbed" => LoadType.GeneralFreight,
            "dry van" => LoadType.GeneralFreight,
            "reefer" => LoadType.RefrigeratedGoods,
            "car carrier" or "auto carrier" or "car hauler" => LoadType.Vehicle,
            "tanker" => LoadType.Liquid,
            _ => LoadType.GeneralFreight
        };

        var load = Load.Create(
            name: $"Load Board - {listing.BrokerName ?? listing.ProviderType.ToString()}",
            type: loadType,
            deliveryCost: bookedRate,
            originAddress: listing.OriginAddress,
            originLocation: listing.OriginLocation,
            destinationAddress: listing.DestinationAddress,
            destinationLocation: listing.DestinationLocation,
            customer: customer,
            assignedTruck: truck,
            assignedDispatcher: dispatcher
        );

        load.ExternalSourceProvider = listing.ProviderType;
        load.ExternalSourceId = listing.ExternalListingId;
        load.ExternalBrokerReference = bookingResult.ExternalConfirmationId;

        await tenantUow.Repository<Load>().AddAsync(load, ct);

        listing.Status = LoadBoardListingStatus.Booked;
        listing.BookedAt = DateTime.UtcNow;
        listing.LoadId = load.Id;
        listing.Notes = req.Notes;

        // Booking the listing settles any open negotiation on it, whatever rate it ends at.
        negotiation?.MarkAccepted(load.Id);

        await tenantUow.SaveChangesAsync(ct);

        // A won thread still owns a live reply address; leaving it routing is an open door.
        if (negotiation is not null)
        {
            await routeRegistry.RevokeAsync([negotiation.ReplyToken], ct);
            await broadcastService.BroadcastNegotiationAsync(
                tenantUow.GetCurrentTenant().Id, negotiation.ToDto(listing));
        }

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

    /// <summary>
    /// A negotiated rate is only meaningful against the thread that produced it, and the rate the
    /// load is actually booked at may never undercut the floor that thread opened at - the floor
    /// snapshot, not a fresh lookup, since the lane floor may have moved since the offer went out.
    /// Omitting <paramref name="negotiatedRate"/> books at the listing's own rate, which is checked
    /// the same way rather than skipping the floor.
    /// </summary>
    private static Result CheckBookingRate(
        decimal? negotiatedRate, decimal bookedRate, RateNegotiation? negotiation)
    {
        if (negotiation is null)
        {
            return negotiatedRate is null
                ? Result.Ok()
                : Result.Fail(
                    "There is no open rate negotiation on this listing, so there is no negotiated rate to book at.");
        }

        if (negotiation.FloorTotalRate is not { } floor)
        {
            // The thread opened on a per-mile floor with no distance to convert it, so there is no
            // total to compare against. Refuse rather than book an unchecked rate.
            return Result.Fail(
                "This negotiation opened against a per-mile floor on a listing with no distance, so " +
                "the booking rate cannot be checked. Set a minimum total rate on the lane floor.",
                ErrorCodes.NegotiationFloorMissing);
        }

        if (bookedRate < floor.Amount)
        {
            return Result.Fail(
                $"The booking rate {bookedRate:N2} is below the floor of {floor.Amount:N2} this negotiation opened against.",
                ErrorCodes.NegotiationBelowFloor);
        }

        return Result.Ok();
    }
}
