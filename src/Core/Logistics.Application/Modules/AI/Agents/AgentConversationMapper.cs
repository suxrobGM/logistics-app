using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Application.Modules.AI.Agents;

[Mapper]
public static partial class AgentConversationMapper
{
    [MapperIgnoreSource(nameof(AgentConversation.Messages))]
    [MapperIgnoreTarget(nameof(AgentConversationDto.Messages))]
    [MapperIgnoreTarget(nameof(AgentConversationDto.Decisions))]
    [MapperIgnoreTarget(nameof(AgentConversationDto.Sessions))]
    public static partial AgentConversationDto ToDto(this AgentConversation entity);

    [MapperIgnoreSource(nameof(AgentMessage.Conversation))]
    [MapperIgnoreTarget(nameof(AgentMessageDto.SentByName))]
    [MapProperty(nameof(AgentMessage.DisplayText), nameof(AgentMessageDto.Text))]
    public static partial AgentMessageDto ToDto(this AgentMessage entity);

    extension(AgentMessage entity)
    {
        public AgentMessageDto ToDto(string? senderName)
        {
            var dto = entity.ToDto();
            dto.SentByName = senderName;
            return dto;
        }
    }
}
