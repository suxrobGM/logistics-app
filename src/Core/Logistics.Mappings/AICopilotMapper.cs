using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class AICopilotMapper
{
    [MapperIgnoreSource(nameof(AICopilotConversation.DomainEvents))]
    [MapperIgnoreSource(nameof(AICopilotConversation.Messages))]
    [MapperIgnoreSource(nameof(AICopilotConversation.CreatedById))]
    [MapperIgnoreSource(nameof(AICopilotConversation.TurnStartedAt))]
    [MapperIgnoreSource(nameof(AICopilotConversation.CreatedBy))]
    [MapperIgnoreSource(nameof(AICopilotConversation.UpdatedAt))]
    [MapperIgnoreSource(nameof(AICopilotConversation.UpdatedBy))]
    public static partial AICopilotConversationDto ToDto(this AICopilotConversation entity);

    [MapperIgnoreSource(nameof(AICopilotMessage.DomainEvents))]
    [MapperIgnoreSource(nameof(AICopilotMessage.Conversation))]
    [MapperIgnoreSource(nameof(AICopilotMessage.ContentJson))]
    [MapProperty(nameof(AICopilotMessage.DisplayText), nameof(AICopilotMessageDto.Text))]
    public static partial AICopilotMessageDto ToDto(this AICopilotMessage entity);
}
