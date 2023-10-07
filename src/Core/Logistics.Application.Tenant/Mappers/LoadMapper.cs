using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Mappers;

public static class LoadMapper
{
    public static LoadDto ToDto(this Load entity)
    {
        var loadDto = new LoadDto
        {
            Id = entity.Id,
            RefId = entity.RefId,
            Name = entity.Name,
            OriginAddress = entity.OriginAddress,
            OriginAddressLat = entity.OriginAddressLat,
            OriginAddressLong = entity.OriginAddressLong,
            DestinationAddress = entity.DestinationAddress,
            DestinationAddressLat = entity.DestinationAddressLat,
            DestinationAddressLong = entity.DestinationAddressLong,
            DispatchedDate = entity.DispatchedDate,
            PickUpDate = entity.PickUpDate,
            DeliveryDate = entity.DeliveryDate,
            DeliveryCost = entity.DeliveryCost,
            Distance = entity.Distance,
            CanConfirmPickUp = entity.CanConfirmPickUp,
            CanConfirmDelivery = entity.CanConfirmDelivery,
            Status = entity.GetStatus(),
            AssignedDispatcherId = entity.AssignedDispatcherId,
            AssignedDispatcherName = entity.AssignedDispatcher?.GetFullName(),
            AssignedTruckId = entity.AssignedTruckId,
            AssignedTruckNumber = entity.AssignedTruck?.TruckNumber,
            AssignedTruckDriversName = entity.AssignedTruck?.Drivers.Select(i => i.GetFullName()),
            CurrentLocation = entity.AssignedTruck?.LastKnownLocation
        };
        
        return loadDto;
    }
}
