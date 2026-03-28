using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class DispatchDecisionMapper
{
    [MapperIgnoreSource(nameof(DispatchDecision.Session))]
    [MapperIgnoreTarget(nameof(DispatchDecisionDto.LoadName))]
    [MapperIgnoreTarget(nameof(DispatchDecisionDto.TruckNumber))]
    public static partial DispatchDecisionDto ToDto(this DispatchDecision entity);
}
