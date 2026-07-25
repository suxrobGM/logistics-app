using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class AiDispatchPolicyMapper
{
    // ModelUsed / GenerationCostUsd stay out of the DTO - platform observability only.
    [MapperIgnoreSource(nameof(AiDispatchPolicy.DomainEvents))]
    [MapperIgnoreSource(nameof(AiDispatchPolicy.ModelUsed))]
    [MapperIgnoreSource(nameof(AiDispatchPolicy.GenerationCostUsd))]
    [MapperIgnoreSource(nameof(AiDispatchPolicy.LastDecisionAt))]
    [MapperIgnoreSource(nameof(AiDispatchPolicy.LastRunAt))]
    [MapperIgnoreSource(nameof(AiDispatchPolicy.CreatedAt))]
    [MapperIgnoreSource(nameof(AiDispatchPolicy.CreatedBy))]
    [MapperIgnoreSource(nameof(AiDispatchPolicy.UpdatedAt))]
    [MapperIgnoreSource(nameof(AiDispatchPolicy.UpdatedBy))]
    public static partial AiDispatchPolicyDto ToDto(this AiDispatchPolicy entity);
}
