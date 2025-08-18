using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class LoadMapper
{
    [UserMapping(Default = true)]
    public static LoadDto ToDto(this Load entity)
    {
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
            Status = entity.Status,
            AssignedDispatcherId = entity.AssignedDispatcherId,
            AssignedDispatcherName = entity.AssignedDispatcher?.GetFullName(),
            AssignedTruckId = entity.AssignedTruckId,
            AssignedTruckNumber = entity.AssignedTruck?.Number,
            AssignedTruckDriversName = entity.AssignedTruck?.GetDriversNames(),
            Customer = entity.Customer?.ToDto(),
            Invoices = entity.Invoices.Select(i => i.ToDto()),
            TripId = entity.TripStop?.Trip.Id,
            TripName = entity.TripStop?.Trip.Name,
            TripNumber = entity.TripStop?.Trip.Number
        };

        if (entity.AssignedTruck?.CurrentAddress.IsNotNull() ?? false)
        {
            dto.CurrentAddress = entity.AssignedTruck.CurrentAddress;
            dto.CurrentLocation = entity.AssignedTruck.CurrentLocation;
        }

        return dto;
    }

    [MapperIgnoreSource(nameof(Load.DomainEvents))]
    [MapperIgnoreSource(nameof(Load.Invoices))]
    [MapperIgnoreSource(nameof(Load.AssignedDispatcher))]
    [MapperIgnoreSource(nameof(Load.AssignedDispatcherId))]
    [MapperIgnoreSource(nameof(Load.AssignedTruck))]
    [MapperIgnoreSource(nameof(Load.AssignedTruckId))]
    [MapperIgnoreSource(nameof(Load.TripStop))]
    [MapperIgnoreSource(nameof(Load.Type))]
    [MapperIgnoreSource(nameof(Load.OriginLocation))]
    [MapperIgnoreSource(nameof(Load.DestinationLocation))]
    [MapProperty(nameof(Load.OriginLocation), nameof(TripLoadDto.OriginLocation))]
    [MapProperty(nameof(Load.DestinationLocation), nameof(TripLoadDto.DestinationLocation))]
    public static partial TripLoadDto ToTripLoadDto(this Load load);
}
