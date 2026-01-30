using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class DriverBehaviorMapper
{
    public static DriverBehaviorEventDto ToDto(this DriverBehaviorEvent entity)
    {
        return new DriverBehaviorEventDto
        {
            Id = entity.Id,
            DriverId = entity.EmployeeId,
            DriverName = entity.Employee?.GetFullName() ?? string.Empty,
            TruckId = entity.TruckId,
            TruckNumber = entity.Truck?.Number,
            EventType = entity.EventType,
            EventTypeDisplay = entity.EventType.GetDescription(),
            OccurredAt = entity.OccurredAt,
            ProviderType = entity.ProviderType,
            ProviderTypeDisplay = entity.ProviderType.GetDescription(),
            Latitude = entity.Latitude,
            Longitude = entity.Longitude,
            Location = entity.Location,
            SpeedMph = entity.SpeedMph,
            SpeedLimitMph = entity.SpeedLimitMph,
            GForce = entity.GForce,
            DurationSeconds = entity.DurationSeconds,
            IsReviewed = entity.IsReviewed,
            ReviewedById = entity.ReviewedById,
            ReviewedByName = entity.ReviewedBy?.GetFullName(),
            ReviewedAt = entity.ReviewedAt,
            ReviewNotes = entity.ReviewNotes,
            IsDismissed = entity.IsDismissed
        };
    }
}
