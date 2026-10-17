using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Messaging.Commands;

/// <summary>
/// Marks a conversation read for one reader: moves their <c>LastReadAt</c>, which the unread counts use,
/// and receipts each message they had not read.
/// </summary>
public class MarkConversationReadCommand : ICommand
{
    public Guid ConversationId { get; set; }
    public Guid ReadById { get; set; }
}
