using Logistics.Application.Services.Realtime;
using Logistics.Shared.Models.Messaging;

namespace Logistics.Infrastructure.Communications.SignalR.Services;

internal sealed class SignalRMessagingService : IRealtimeMessagingService
{
    public Task BroadcastMessageToConversationAsync(Guid conversationId, MessageDto message,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task BroadcastMessageReadAsync(Guid conversationId, Guid messageId, string readById,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task BroadcastTypingIndicatorAsync(Guid conversationId, string userId, bool isTyping,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task BroadcastUserJoinedAsync(Guid conversationId, string userId,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task BroadcastUserLeftAsync(Guid conversationId, string userId,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
