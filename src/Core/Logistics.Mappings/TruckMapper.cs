using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class TruckMapper
{
    /// <summary>
    ///     Maps a Truck entity to TruckDto without loads.
    /// </summary>
    public static TruckDto ToDto(this Truck entity)
    {
        return new TruckDto
        {
            Id = entity.Id,
            Number = entity.Number,
            Type = entity.Type,
            Status = entity.Status,
            Make = entity.Make,
            Model = entity.Model,
            Year = entity.Year,
            Vin = entity.Vin,
            LicensePlate = entity.LicensePlate,
            LicensePlateState = entity.LicensePlateState,
            AdrEquipment = entity.AdrEquipment.ToDto(),
            IsHazmatPlacarded = entity.IsHazmatPlacarded,
            CurrentLocation = entity.CurrentLocation,
            CurrentAddress = entity.CurrentAddress,
            VehicleCapacity = entity.VehicleCapacity,
            Loads = [],
            MainDriver = entity.MainDriver?.ToDto(),
            SecondaryDriver = entity.SecondaryDriver?.ToDto(),
        };
    }

    public static AdrEquipmentDto ToDto(this AdrEquipment entity) => new()
    {
        IsAdrCertified = entity.IsAdrCertified,
        AdrCertExpiresAt = entity.AdrCertExpiresAt,
        AllowedClasses = entity.AllowedClasses.ToClasses(),
        OrangePlateNumber = entity.OrangePlateNumber
    };

    public static AdrEquipment ToDomain(this AdrEquipmentDto dto) => new()
    {
        IsAdrCertified = dto.IsAdrCertified,
        AdrCertExpiresAt = dto.AdrCertExpiresAt,
        AllowedClasses = dto.AllowedClasses.ToFlags(),
        OrangePlateNumber = dto.OrangePlateNumber
    };

    /// <summary>
    ///     Maps a Truck entity to TruckDto with loads.
    /// </summary>
    public static TruckDto ToDto(this Truck entity, IEnumerable<LoadDto> loads)
    {
        var dto = entity.ToDto();
        dto.Loads = loads;
        return dto;
    }
}
