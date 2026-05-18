using Logistics.Application.Abstractions;
using Logistics.Shared.Models;
using Logistics.Shared.Models.Messaging;

namespace Logistics.Application.Modules.Integrations.Messaging.Commands;

public class CreateConversationCommand : ICommand<Result<ConversationDto>>
{
    public string? Name { get; set; }
    public Guid? LoadId { get; set; }
    public required List<Guid> ParticipantIds { get; set; }
}
