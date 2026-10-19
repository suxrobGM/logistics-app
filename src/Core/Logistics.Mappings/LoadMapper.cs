using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
[UseStaticMapper(typeof(CustomerMapper))]
public static partial class LoadMapper
{
    /// <summary>
    /// Related values come from <paramref name="lookup"/> only - there is no navigation-property
    /// fallback, so a handler that skips the resolver gets blank fields instead of silently paying
    /// a lazy SELECT per row. Single-row callers resolve a one-load batch.
    /// </summary>
    public static LoadDto ToDto(this Load entity, LoadLookup lookup)
    {
        var container = lookup.FindContainer(entity.ContainerId);
        var originTerminal = lookup.FindTerminal(entity.OriginTerminalId);
        var destinationTerminal = lookup.FindTerminal(entity.DestinationTerminalId);
        var truck = lookup.FindTruck(entity.AssignedTruckId);
        var trip = lookup.FindTrip(entity.Id);

        var dto = new LoadDto
        {
            Id = entity.Id,
            Number = entity.Number,
            Name = entity.Name,
            Type = entity.Type,
            OriginAddress = entity.OriginAddress,
            OriginLocation = entity.OriginLocation,
            DestinationAddress = entity.DestinationAddress,
            DestinationLocation = entity.DestinationLocation,
            DispatchedAt = entity.DispatchedAt,
            CreatedAt = entity.CreatedAt,
            PickedUpAt = entity.PickedUpAt,
            DeliveredAt = entity.DeliveredAt,
            DeliveryCost = entity.DeliveryCost,
            Distance = entity.Distance,
            CanConfirmPickUp = entity.CanConfirmPickUp,
            CanConfirmDelivery = entity.CanConfirmDelivery,
            IsInProximity = entity.IsInProximity,
            Source = entity.Source,
            RequestedPickupDate = entity.RequestedPickupDate,
            RequestedDeliveryDate = entity.RequestedDeliveryDate,
            Notes = entity.Notes,
            HazmatClass = entity.HazmatClass,
            UnNumber = entity.UnNumber,
            FreightClass = entity.FreightClass,
            NmfcCode = entity.NmfcCode,
            PalletCount = entity.PalletCount,
            PieceCount = entity.PieceCount,
            Weight = entity.Weight,
            Dimensions = entity.Dimensions,
            IsStackable = entity.IsStackable,
            FuelSurcharge = entity.FuelSurcharge,
            ContainerId = entity.ContainerId,
            ContainerNumber = container?.Number,
            ContainerIsoType = container?.IsoType,
            ContainerLastFreeDay = container?.LastFreeDay,
            ContainerIsOnHold = container?.IsOnHold,
            OriginTerminalId = entity.OriginTerminalId,
            OriginTerminalName = originTerminal?.Name,
            OriginTerminalCode = originTerminal?.Code,
            DestinationTerminalId = entity.DestinationTerminalId,
            DestinationTerminalName = destinationTerminal?.Name,
            DestinationTerminalCode = destinationTerminal?.Code,
            Status = entity.Status,
            AssignedDispatcherId = entity.AssignedDispatcherId,
            AssignedDispatcherName = lookup.FindEmployeeName(entity.AssignedDispatcherId),
            AssignedTruckId = entity.AssignedTruckId,
            AssignedTruckNumber = truck?.Number,
            AssignedTruckDriversName = truck?.DriversNames,
            Vehicles = entity.Vehicles.ToDto(),
            Customer = lookup.FindCustomer(entity.CustomerId),
            Invoice = lookup.FindInvoice(entity.Id),
            TripId = trip?.Id,
            TripName = trip?.Name,
            TripNumber = trip?.Number
        };

        if (truck?.CurrentAddress is not null)
        {
            dto.CurrentAddress = truck.CurrentAddress;
            dto.CurrentLocation = truck.CurrentLocation;
        }

        return dto;
    }

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapperIgnoreSource(nameof(Load.DomainEvents))]
    [MapperIgnoreSource(nameof(Load.Invoice))]
    [MapperIgnoreSource(nameof(Load.AssignedDispatcher))]
    [MapperIgnoreSource(nameof(Load.AssignedDispatcherId))]
    [MapperIgnoreSource(nameof(Load.AssignedTruck))]
    [MapperIgnoreSource(nameof(Load.AssignedTruckId))]
    [MapperIgnoreSource(nameof(Load.TripStops))]
    [MapProperty(nameof(Load.OriginLocation), nameof(TripLoadDto.OriginLocation))]
    [MapProperty(nameof(Load.DestinationLocation), nameof(TripLoadDto.DestinationLocation))]
    [MapProperty(nameof(Load.Type), nameof(TripLoadDto.Type))]
    public static partial TripLoadDto ToTripLoadDto(this Load load);
}
