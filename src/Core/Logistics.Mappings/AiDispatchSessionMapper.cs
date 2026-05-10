using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class AiDispatchSessionMapper
{
    [MapperIgnoreSource(nameof(AiDispatchSession.DomainEvents))]
    [MapperIgnoreSource(nameof(AiDispatchSession.Decisions))]
    public static partial AiDispatchSessionDto ToDto(this AiDispatchSession entity);

    public static AiDispatchSessionDto ToDtoWithDecisions(this AiDispatchSession entity)
    {
        var dto = entity.ToDto();
        dto.Decisions = entity.Decisions.Select(d => d.ToDto()).ToList();
        return dto;
    }
}
