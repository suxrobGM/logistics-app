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
            OriginAddress = entity.OriginAddress.ToDto(),
            OriginAddressLat = entity.OriginAddressLat,
            OriginAddressLong = entity.OriginAddressLong,
            DestinationAddress = entity.DestinationAddress.ToDto(),
            DestinationAddressLat = entity.DestinationAddressLat,
            DestinationAddressLong = entity.DestinationAddressLong,
            DispatchedDate = entity.DispatchedDate,
            PickUpDate = entity.PickUpDate,
            DeliveryDate = entity.DeliveryDate,
            DeliveryCost = entity.DeliveryCost,
            Distance = entity.Distance,
            CanConfirmPickUp = entity.CanConfirmPickUp,
            CanConfirmDelivery = entity.CanConfirmDelivery,
            Status = entity.Status,
            AssignedDispatcherId = entity.AssignedDispatcherId,
            AssignedDispatcherName = entity.AssignedDispatcher?.GetFullName(),
            AssignedTruckId = entity.AssignedTruckId,
            AssignedTruckNumber = entity.AssignedTruck?.Number,
            AssignedTruckDriversName = entity.AssignedTruck?.Drivers.Select(i => i.GetFullName()),
            Customer = entity.Customer?.ToDto(),
            Invoices = entity.Invoices.Select(i => i.ToDto()),
            TripId = entity.TripStop?.Trip.Id,
            TripName = entity.TripStop?.Trip.Name,
            TripNumber = entity.TripStop?.Trip.Number,
        };
        
        if (entity.AssignedTruck?.CurrentLocation.IsNotNull() ?? false)
        {
            dto.CurrentLocation = entity.AssignedTruck.CurrentLocation.ToDto();
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
    [MapperIgnoreSource(nameof(Load.OriginAddressLat))]
    [MapperIgnoreSource(nameof(Load.OriginAddressLong))]
    [MapperIgnoreSource(nameof(Load.DestinationAddressLat))]
    [MapperIgnoreSource(nameof(Load.DestinationAddressLong))]
    public static partial TripLoadDto ToTripLoadDto(this Load load);
}
