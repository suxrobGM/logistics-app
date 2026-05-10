using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class AiDispatchDecisionMapper
{
    [MapperIgnoreSource(nameof(AiDispatchDecision.Session))]
    [MapperIgnoreTarget(nameof(AiDispatchDecisionDto.LoadName))]
    [MapperIgnoreTarget(nameof(AiDispatchDecisionDto.TruckNumber))]
    public static partial AiDispatchDecisionDto ToDto(this AiDispatchDecision entity);
}
