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
        CancellationToken ct = default);

    Task BroadcastMessageReadAsync(
        Guid conversationId,
        Guid messageId,
        string readById,
        CancellationToken ct = default);

    Task BroadcastTypingIndicatorAsync(
        Guid conversationId,
        string userId,
        bool isTyping,
        CancellationToken ct = default);

    Task BroadcastUserJoinedAsync(
        Guid conversationId,
        string userId,
        CancellationToken ct = default);

    Task BroadcastUserLeftAsync(
        Guid conversationId,
        string userId,
        CancellationToken ct = default);
}
