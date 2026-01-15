using Logistics.Application.Abstractions;
using Logistics.Shared.Models;
using Logistics.Shared.Models.Messaging;

namespace Logistics.Application.Queries;

public class GetMessagesQuery : IAppRequest<Result<MessageDto[]>>
{
    public Guid ConversationId { get; set; }

    /// <summary>
    /// Number of messages to retrieve (default 50).
    /// </summary>
    public int Limit { get; set; } = 50;

    /// <summary>
    /// Offset for pagination (default 0).
    /// </summary>
    public int Offset { get; set; } = 0;

    /// <summary>
    /// Get messages before this date (for infinite scroll).
    /// </summary>
    public DateTime? Before { get; set; }
}
