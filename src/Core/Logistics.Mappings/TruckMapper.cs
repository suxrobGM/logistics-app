﻿using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class TruckMapper
{
    public static TruckDto ToDto(this Truck entity, IEnumerable<LoadDto> loads)
    {
        var dto = new TruckDto
        {
            Id = entity.Id,
            Number = entity.Number,
            Type = entity.Type,
            Status = entity.Status,
            CurrentLocationLat = entity.CurrentLocationLat,
            CurrentLocationLong = entity.CurrentLocationLong,
            Loads = loads,
            MainDriver = entity.MainDriver?.ToDto(),
            SecondaryDriver = entity.SecondaryDriver?.ToDto(),
        };

        if (entity.CurrentLocation.IsNotNull())
        {
            dto.CurrentLocation = entity.CurrentLocation;
        }
        return dto;
    }
}
