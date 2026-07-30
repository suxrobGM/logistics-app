using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class AIDispatchSessionMapper
{
    [MapperIgnoreSource(nameof(AgentSession.DomainEvents))]
    [MapperIgnoreSource(nameof(AgentSession.Decisions))]
    [MapperIgnoreSource(nameof(AgentSession.Type))]
    [MapperIgnoreSource(nameof(AgentSession.ConversationId))]
    [MapperIgnoreSource(nameof(AgentSession.Conversation))]
    public static partial AgentSessionDto ToDto(this AgentSession entity);

    public static AgentSessionDto ToDtoWithDecisions(this AgentSession entity)
    {
        var dto = entity.ToDto();
        dto.Decisions = entity.Decisions.Select(d => d.ToDto()).ToList();
        return dto;
    }
}
