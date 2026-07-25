using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class AIDispatchPolicyMapper
{
    // ModelUsed / GenerationCostUsd stay out of the DTO - platform observability only.
    [MapperIgnoreSource(nameof(AIDispatchPolicy.DomainEvents))]
    [MapperIgnoreSource(nameof(AIDispatchPolicy.ModelUsed))]
    [MapperIgnoreSource(nameof(AIDispatchPolicy.GenerationCostUsd))]
    [MapperIgnoreSource(nameof(AIDispatchPolicy.LastDecisionAt))]
    [MapperIgnoreSource(nameof(AIDispatchPolicy.LastRunAt))]
    [MapperIgnoreSource(nameof(AIDispatchPolicy.CreatedAt))]
    [MapperIgnoreSource(nameof(AIDispatchPolicy.CreatedBy))]
    [MapperIgnoreSource(nameof(AIDispatchPolicy.UpdatedAt))]
    [MapperIgnoreSource(nameof(AIDispatchPolicy.UpdatedBy))]
    public static partial AIDispatchPolicyDto ToDto(this AIDispatchPolicy entity);
}
