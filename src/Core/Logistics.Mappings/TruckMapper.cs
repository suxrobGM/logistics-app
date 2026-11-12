using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class TruckMapper
{
    extension(Truck entity)
    {
        /// <summary>
        ///     Maps a Truck entity to TruckDto without loads.
        /// </summary>
        public TruckDto ToDto()
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
                CurrentTrailerId = entity.CurrentTrailerId,
            };
        }

        /// <summary>
        ///     Maps a Truck entity to TruckDto with loads.
        /// </summary>
        public TruckDto ToDto(IEnumerable<LoadDto> loads)
        {
            var dto = entity.ToDto();
            dto.Loads = loads;
            return dto;
        }
    }

    extension(AdrEquipment entity)
    {
        public AdrEquipmentDto ToDto() => new()
        {
            IsAdrCertified = entity.IsAdrCertified,
            AdrCertExpiresAt = entity.AdrCertExpiresAt,
            AllowedClasses = entity.AllowedClasses.ToClasses(),
            OrangePlateNumber = entity.OrangePlateNumber
        };
    }

    extension(AdrEquipmentDto dto)
    {
        public AdrEquipment ToDomain() => new()
        {
            IsAdrCertified = dto.IsAdrCertified,
            AdrCertExpiresAt = dto.AdrCertExpiresAt,
            AllowedClasses = dto.AllowedClasses.ToFlags(),
            OrangePlateNumber = dto.OrangePlateNumber
        };
    }

}
