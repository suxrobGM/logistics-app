using Logistics.Application.Abstractions;
using Logistics.Shared.Models;
using Logistics.Shared.Models.Messaging;

namespace Logistics.Application.Modules.Integrations.Messaging.Queries;

public class GetConversationsQuery : IQuery<Result<ConversationDto[]>>
{
    /// <summary>
    /// Filter conversations by participant employee ID.
    /// </summary>
    public Guid? ParticipantId { get; set; }

    /// <summary>
    /// Filter conversations by load ID.
    /// </summary>
    public Guid? LoadId { get; set; }
}
