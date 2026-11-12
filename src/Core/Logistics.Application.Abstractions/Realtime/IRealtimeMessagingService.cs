using Logistics.Shared.Models.Messaging;
using Logistics.Application.Abstractions.Realtime;

namespace Logistics.Application.Abstractions.Realtime;

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
