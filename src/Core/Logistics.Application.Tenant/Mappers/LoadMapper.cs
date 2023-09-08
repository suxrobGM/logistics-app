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
            DestinationAddress = entity.DestinationAddress,
            DispatchedDate = entity.DispatchedDate,
            PickUpDate = entity.PickUpDate,
            DeliveryDate = entity.DeliveryDate,
            DeliveryCost = entity.DeliveryCost,
            Distance = entity.Distance,
            Status = (LoadStatusDto)entity.Status,
            AssignedDispatcherId = entity.AssignedDispatcherId,
            AssignedDispatcherName = entity.AssignedDispatcher?.GetFullName(),
            AssignedTruckId = entity.AssignedTruckId,
            AssignedTruckNumber = entity.AssignedTruck?.TruckNumber,
            AssignedTruckDriversId = entity.AssignedTruck?.Drivers.Select(i => i.Id),
            AssignedTruckDriversName = entity.AssignedTruck?.Drivers.Select(i => i.GetFullName()) 
        };
    }
}
