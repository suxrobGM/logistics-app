using Logistics.Application.Abstractions;
using Logistics.Shared.Models;
using Logistics.Shared.Models.Messaging;

namespace Logistics.Application.Commands;

public class CreateConversationCommand : IAppRequest<Result<ConversationDto>>
{
    public string? Name { get; set; }
    public Guid? LoadId { get; set; }
    public required List<Guid> ParticipantIds { get; set; }
}
