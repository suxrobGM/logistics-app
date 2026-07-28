using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class AIDispatchSessionMapper
{
    [MapperIgnoreSource(nameof(AIDispatchSession.DomainEvents))]
    [MapperIgnoreSource(nameof(AIDispatchSession.Decisions))]
    [MapperIgnoreSource(nameof(AIDispatchSession.Type))]
    [MapperIgnoreSource(nameof(AIDispatchSession.ConversationId))]
    [MapperIgnoreSource(nameof(AIDispatchSession.Conversation))]
    public static partial AIDispatchSessionDto ToDto(this AIDispatchSession entity);

    public static AIDispatchSessionDto ToDtoWithDecisions(this AIDispatchSession entity)
    {
        var dto = entity.ToDto();
        dto.Decisions = entity.Decisions.Select(d => d.ToDto()).ToList();
        return dto;
    }
}
