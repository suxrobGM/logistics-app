using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Application.Modules.AI.Negotiation;

[Mapper]
public static partial class LaneRateFloorMapper
{
    public static partial LaneRateFloorDto ToDto(this LaneRateFloor entity);
}
