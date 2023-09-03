using Logistics.Models;

namespace Logistics.Application.Tenant.Mappers;

public class LoadMapper : IMapper<Load, LoadDto>
{
    public static Load? ToEntity(LoadDto? dto)
    {
        if (dto is null)
            return null;
        
        return new Load
        {
            Id = dto.Id!,
            RefId = dto.RefId,
            Name = dto.Name,
            SourceAddress = dto.SourceAddress,
            DestinationAddress = dto.DestinationAddress,
            DispatchedDate = dto.DispatchedDate,
            PickUpDate = dto.PickUpDate,
            DeliveryDate = dto.DeliveryDate,
            DeliveryCost = dto.DeliveryCost,
            Distance = dto.Distance,
            Status = (Domain.Enums.LoadStatus)dto.Status,
            AssignedDispatcherId = dto.AssignedDispatcherId,
            AssignedDriverId = dto.Id,
            AssignedTruckId = dto.AssignedTruckId
        };
    }

    public static LoadDto? ToDto(Load? entity)
    {
        if (entity is null)
            return null;
        
        return new LoadDto
        {
            Id = entity.Id,
            RefId = entity.RefId,
            Name = entity.Name,
            SourceAddress = entity.SourceAddress,
            DestinationAddress = entity.DestinationAddress,
            DispatchedDate = entity.DispatchedDate,
            PickUpDate = entity.PickUpDate,
            DeliveryDate = entity.DeliveryDate,
            DeliveryCost = entity.DeliveryCost,
            Distance = entity.Distance,
            Status = (LoadStatus)entity.Status,
            AssignedDispatcherId = entity.AssignedDispatcherId,
            AssignedDriverId = entity.Id,
            AssignedTruckId = entity.AssignedTruckId
        };
    }
}