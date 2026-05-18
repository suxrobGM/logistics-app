using Logistics.Application.Abstractions;
using Logistics.Shared.Models;
using Logistics.Shared.Models.Messaging;

namespace Logistics.Application.Modules.Integrations.Messaging.Commands;

public class SendMessageCommand : ICommand<Result<MessageDto>>
{
    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }
    public required string Content { get; set; }
}
