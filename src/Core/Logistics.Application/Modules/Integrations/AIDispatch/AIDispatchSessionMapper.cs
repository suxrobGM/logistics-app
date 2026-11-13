using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Application.Modules.Integrations.AIDispatch;

[Mapper]
public static partial class AIDispatchSessionMapper
{
    [MapperIgnoreSource(nameof(AgentSession.Decisions))]
    [MapperIgnoreSource(nameof(AgentSession.Conversation))]
    [MapProperty(nameof(AgentSession.CreatedAt), nameof(AgentSessionDto.StartedAt))]
    public static partial AgentSessionDto ToDto(this AgentSession entity);
}
