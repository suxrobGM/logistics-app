using Logistics.Models;

namespace Logistics.Application.Tenant.Mappers;

public static class LoadMapper
{
    public static LoadDto ToDto(this Load entity)
    {
        return new LoadDto
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
            Status = (LoadStatusDto)entity.Status,
            AssignedDispatcherId = entity.AssignedDispatcherId,
            AssignedDispatcherName = entity.AssignedDispatcher?.GetFullName(),
            AssignedTruck = entity.AssignedTruck?.ToDto(new List<LoadDto>())
        };
    }
}
