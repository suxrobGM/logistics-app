using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class LaneRateFloorMapper
{
    public static partial LaneRateFloorDto ToDto(this LaneRateFloor entity);
}
