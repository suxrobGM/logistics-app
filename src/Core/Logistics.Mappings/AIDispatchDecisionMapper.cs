using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class AIDispatchDecisionMapper
{
    [MapperIgnoreSource(nameof(AIDispatchDecision.Session))]
    [MapperIgnoreTarget(nameof(AIDispatchDecisionDto.LoadName))]
    [MapperIgnoreTarget(nameof(AIDispatchDecisionDto.TruckNumber))]
    public static partial AIDispatchDecisionDto ToDto(this AIDispatchDecision entity);
}
