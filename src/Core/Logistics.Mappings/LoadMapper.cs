using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class LoadMapper
{
    [UserMapping(Default = true)]
    public static LoadDto ToDto(this Load entity) => entity.ToDto(LoadIntermodalLookup.Empty);

    /// <summary>
    /// Maps with container / terminal values already resolved for the page. Anything the lookup does
    /// not carry falls back to the navigation property and lazy-loads, so list handlers must pass a
    /// populated <see cref="LoadIntermodalLookup"/>.
    /// </summary>
    public static LoadDto ToDto(this Load entity, LoadIntermodalLookup intermodal)
    {
        var container = intermodal.FindContainer(entity.ContainerId);
        var originTerminal = intermodal.FindTerminal(entity.OriginTerminalId);
        var destinationTerminal = intermodal.FindTerminal(entity.DestinationTerminalId);

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
            IsHazmat = entity.IsHazmat,
            HazmatClass = entity.HazmatClass,
            UnNumber = entity.UnNumber,
            ContainerId = entity.ContainerId,
            ContainerNumber = container?.Number ?? entity.Container?.Number,
            ContainerIsoType = container?.IsoType ?? entity.Container?.IsoType,
            OriginTerminalId = entity.OriginTerminalId,
            OriginTerminalName = originTerminal?.Name ?? entity.OriginTerminal?.Name,
            OriginTerminalCode = originTerminal?.Code ?? entity.OriginTerminal?.Code,
            DestinationTerminalId = entity.DestinationTerminalId,
            DestinationTerminalName = destinationTerminal?.Name ?? entity.DestinationTerminal?.Name,
            DestinationTerminalCode = destinationTerminal?.Code ?? entity.DestinationTerminal?.Code,
            Status = entity.Status,
            AssignedDispatcherId = entity.AssignedDispatcherId,
            AssignedDispatcherName = entity.AssignedDispatcher?.GetFullName(),
            AssignedTruckId = entity.AssignedTruckId,
            AssignedTruckNumber = entity.AssignedTruck?.Number,
            AssignedTruckDriversName = entity.AssignedTruck?.GetDriversNames(),
            Customer = entity.Customer?.ToDto(),
            Invoice = entity.Invoice?.ToDto(),
            TripId = entity.TripStops.FirstOrDefault()?.Trip.Id,
            TripName = entity.TripStops.FirstOrDefault()?.Trip.Name,
            TripNumber = entity.TripStops.FirstOrDefault()?.Trip.Number
        };

        if (entity.AssignedTruck?.CurrentAddress is not null)
        {
            dto.CurrentAddress = entity.AssignedTruck.CurrentAddress;
            dto.CurrentLocation = entity.AssignedTruck.CurrentLocation;
        }

        return dto;
    }

    [MapperIgnoreSource(nameof(Load.DomainEvents))]
    [MapperIgnoreSource(nameof(Load.Invoice))]
    [MapperIgnoreSource(nameof(Load.AssignedDispatcher))]
    [MapperIgnoreSource(nameof(Load.AssignedDispatcherId))]
    [MapperIgnoreSource(nameof(Load.AssignedTruck))]
    [MapperIgnoreSource(nameof(Load.AssignedTruckId))]
    [MapperIgnoreSource(nameof(Load.TripStops))]
    [MapperIgnoreSource(nameof(Load.Type))]
    [MapperIgnoreSource(nameof(Load.OriginLocation))]
    [MapperIgnoreSource(nameof(Load.DestinationLocation))]
    [MapProperty(nameof(Load.OriginLocation), nameof(TripLoadDto.OriginLocation))]
    [MapProperty(nameof(Load.DestinationLocation), nameof(TripLoadDto.DestinationLocation))]
    [MapProperty(nameof(Load.Type), nameof(TripLoadDto.Type))]
    public static partial TripLoadDto ToTripLoadDto(this Load load);
}
