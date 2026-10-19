using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class AgentConversationMapper
{
    [MapperIgnoreSource(nameof(AgentConversation.DomainEvents))]
    [MapperIgnoreSource(nameof(AgentConversation.Messages))]
    [MapperIgnoreSource(nameof(AgentConversation.CreatedById))]
    [MapperIgnoreSource(nameof(AgentConversation.Kind))]
    [MapperIgnoreSource(nameof(AgentConversation.TurnStartedAt))]
    [MapperIgnoreSource(nameof(AgentConversation.CreatedBy))]
    [MapperIgnoreSource(nameof(AgentConversation.UpdatedAt))]
    [MapperIgnoreSource(nameof(AgentConversation.UpdatedBy))]
    [MapperIgnoreTarget(nameof(AgentConversationDto.Messages))]
    [MapperIgnoreTarget(nameof(AgentConversationDto.Decisions))]
    [MapperIgnoreTarget(nameof(AgentConversationDto.Sessions))]
    public static partial AgentConversationDto ToDto(this AgentConversation entity);

    [MapperIgnoreSource(nameof(AgentMessage.DomainEvents))]
    [MapperIgnoreSource(nameof(AgentMessage.Conversation))]
    [MapperIgnoreSource(nameof(AgentMessage.ContentJson))]
    [MapperIgnoreTarget(nameof(AgentMessageDto.SentByName))]
    [MapProperty(nameof(AgentMessage.DisplayText), nameof(AgentMessageDto.Text))]
    public static partial AgentMessageDto ToDto(this AgentMessage entity);

    public static AgentMessageDto ToDto(this AgentMessage entity, string? senderName)
    {
        var dto = entity.ToDto();
        dto.SentByName = senderName;
        return dto;
    }
}
