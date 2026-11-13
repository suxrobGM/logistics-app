using Logistics.Shared.Models.Messaging;

namespace Logistics.Application.Abstractions.Communications;

/// <summary>
///     Abstraction for real-time messaging
/// </summary>
public interface IRealtimeMessagingService
{
    Task BroadcastMessageToConversationAsync(
        Guid conversationId,
        MessageDto message,
        CancellationToken ct = default);

    Task BroadcastConversationReadAsync(
        Guid conversationId,
        Guid readById,
        CancellationToken ct = default);
}
