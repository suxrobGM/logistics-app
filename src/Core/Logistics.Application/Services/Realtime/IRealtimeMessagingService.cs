using Logistics.Shared.Models.Messaging;

namespace Logistics.Application.Services.Realtime;

/// <summary>
///     Abstraction for real-time messaging
/// </summary>
public interface IRealtimeMessagingService
{
    Task BroadcastMessageToConversationAsync(
        Guid conversationId,
        MessageDto message,
        CancellationToken cancellationToken = default);

    Task BroadcastMessageReadAsync(
        Guid conversationId,
        Guid messageId,
        string readById,
        CancellationToken cancellationToken = default);

    Task BroadcastTypingIndicatorAsync(
        Guid conversationId,
        string userId,
        bool isTyping,
        CancellationToken cancellationToken = default);

    Task BroadcastUserJoinedAsync(
        Guid conversationId,
        string userId,
        CancellationToken cancellationToken = default);

    Task BroadcastUserLeftAsync(
        Guid conversationId,
        string userId,
        CancellationToken cancellationToken = default);
}
