using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class AIDispatchDecisionMapper
{
    [MapperIgnoreSource(nameof(AgentDecision.Session))]
    [MapperIgnoreTarget(nameof(AgentDecisionDto.LoadName))]
    [MapperIgnoreTarget(nameof(AgentDecisionDto.TruckNumber))]
    public static partial AgentDecisionDto ToDto(this AgentDecision entity);
}
