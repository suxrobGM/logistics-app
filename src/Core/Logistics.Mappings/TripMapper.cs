using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class TripMapper
{
    public static TripDto ToDto(this Trip entity)
    {
        return new TripDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Number = entity.Number,
            OriginAddress = entity.OriginAddress.ToDto(),
            DestinationAddress = entity.DestinationAddress.ToDto(),
            TotalDistance = entity.TotalDistance,
            PlannedStart = entity.PlannedStart,
            ActualStart = entity.ActualStart,
            CompletedAt = entity.CompletedAt,
            Status = entity.Status,
            TruckId = entity.TruckId
        };
    }
}