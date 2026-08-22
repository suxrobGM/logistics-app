using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class LaneRateFloorMapper
{
    [MapperIgnoreSource(nameof(LaneRateFloor.DomainEvents))]
    [MapperIgnoreSource(nameof(LaneRateFloor.CreatedBy))]
    [MapperIgnoreSource(nameof(LaneRateFloor.UpdatedAt))]
    [MapperIgnoreSource(nameof(LaneRateFloor.UpdatedBy))]
    public static partial LaneRateFloorDto ToDto(this LaneRateFloor entity);
}
