using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class LoadVehicleMapper
{
    public static partial LoadVehicleDto ToDto(this LoadVehicle entity);

    public static List<LoadVehicleDto> ToDto(this IEnumerable<LoadVehicle> entities) =>
        [.. entities.Select(ToDto)];
}
