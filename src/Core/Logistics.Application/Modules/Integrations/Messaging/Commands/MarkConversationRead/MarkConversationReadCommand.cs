using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Messaging.Commands;

/// <summary>
/// Marks every message in a conversation read for one reader: moves their <c>LastReadAt</c>, which drives
/// the unread counts, and leaves a receipt on each message they had not read yet.
/// </summary>
public class MarkConversationReadCommand : ICommand
{
    public Guid ConversationId { get; set; }
    public Guid ReadById { get; set; }
}
