using Logistics.Application.Abstractions;
using Logistics.Shared.Models;
using Logistics.Shared.Models.Messaging;

namespace Logistics.Application.Queries;

/// <summary>
/// Gets or creates the tenant-wide group chat.
/// </summary>
public class GetTenantChatQuery : IAppRequest<Result<ConversationDto>>
{
    /// <summary>
    /// The ID of the employee requesting the chat (for unread count calculation).
    /// </summary>
    public Guid EmployeeId { get; set; }
}
