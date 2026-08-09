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
    [MapperIgnoreSource(nameof(AgentSession.TriggeredByUserId))]
    [MapperIgnoreSource(nameof(AgentSession.InputTokensUsed))]
    [MapperIgnoreSource(nameof(AgentSession.OutputTokensUsed))]
    [MapperIgnoreSource(nameof(AgentSession.CacheReadTokens))]
    [MapperIgnoreSource(nameof(AgentSession.CacheCreationTokens))]
    [MapperIgnoreSource(nameof(AgentSession.EstimatedCostUsd))]
    [MapperIgnoreSource(nameof(AgentSession.ModelUsed))]
    [MapperIgnoreSource(nameof(AgentSession.Summary))]
    [MapperIgnoreSource(nameof(AgentSession.IsOverage))]
    public static partial AgentSessionDto ToDto(this AgentSession entity);
}
