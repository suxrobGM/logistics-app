using Logistics.Models;

namespace Logistics.Application.Tenant.Mappers;

public static class LoadMapper
{
    public static Load ToEntity(this LoadDto dto)
    {
        return new Load
        {
            Id = dto.Id!,
            RefId = dto.RefId,
            Name = dto.Name,
            OriginAddress = dto.OriginAddress,
            DestinationAddress = dto.DestinationAddress,
            DispatchedDate = dto.DispatchedDate,
            PickUpDate = dto.PickUpDate,
            DeliveryDate = dto.DeliveryDate,
            DeliveryCost = dto.DeliveryCost,
            Distance = dto.Distance,
            Status = (Domain.Enums.LoadStatus)dto.Status,
            AssignedDispatcherId = dto.AssignedDispatcherId,
            AssignedTruckId = dto.AssignedTruckId
        };
    }

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
            AssignedTruckNumber = entity.AssignedTruck?.TruckNumber
        };
    }
}
