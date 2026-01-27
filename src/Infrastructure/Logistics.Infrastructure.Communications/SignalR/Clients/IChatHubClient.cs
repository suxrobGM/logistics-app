using Logistics.Shared.Models.Messaging;

namespace Logistics.Infrastructure.Communications.SignalR.Clients;

/// <summary>
/// Hub client interface for receiving messaging events.
/// </summary>
public interface IChatHubClient
{
    /// <summary>
    /// Called when a new message is received.
    /// </summary>
    Task ReceiveMessage(MessageDto message);

    /// <summary>
    /// Called when a message is marked as read.
    /// </summary>
    Task MessageRead(Guid messageId, Guid readById);

    /// <summary>
    /// Called when a user starts or stops typing.
    /// </summary>
    Task TypingIndicator(TypingIndicatorDto indicator);

    /// <summary>
    /// Called when a user joins a conversation.
    /// </summary>
    Task UserJoinedConversation(Guid conversationId, Guid userId, string? userName);

    /// <summary>
    /// Called when a user leaves a conversation.
    /// </summary>
    Task UserLeftConversation(Guid conversationId, Guid userId);
}
