using Logistics.Application.Abstractions;
using Logistics.Shared.Models;
using Logistics.Shared.Models.Messaging;

namespace Logistics.Application.Queries;

public class GetConversationsQuery : IAppRequest<Result<ConversationDto[]>>
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
