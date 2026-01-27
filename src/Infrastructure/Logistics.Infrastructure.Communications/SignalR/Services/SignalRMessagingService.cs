using Logistics.Application.Services.Realtime;
using Logistics.Infrastructure.Communications.SignalR.Clients;
using Logistics.Infrastructure.Communications.SignalR.Hubs;
using Logistics.Shared.Models.Messaging;
using Microsoft.AspNetCore.SignalR;

namespace Logistics.Infrastructure.Communications.SignalR.Services;

/// <summary>
///     SignalR implementation of real-time messaging service.
/// </summary>
internal sealed class SignalRMessagingService(IHubContext<ChatHub, IChatHubClient> hubContext)
    : IRealtimeMessagingService
{
    public async Task BroadcastMessageToConversationAsync(
        Guid conversationId,
        MessageDto message,
        CancellationToken ct = default)
    {
        await hubContext.Clients
            .Group($"conversation-{conversationId}")
            .ReceiveMessage(message);
    }

    public async Task BroadcastMessageReadAsync(
        Guid conversationId,
        Guid messageId,
        string readById,
        CancellationToken ct = default)
    {
        await hubContext.Clients
            .Group($"conversation-{conversationId}")
            .MessageRead(messageId, Guid.Parse(readById));
    }

    public async Task BroadcastTypingIndicatorAsync(
        Guid conversationId,
        string userId,
        bool isTyping,
        CancellationToken ct = default)
    {
        var indicator = new TypingIndicatorDto
        {
            ConversationId = conversationId, UserId = Guid.Parse(userId), IsTyping = isTyping
        };

        await hubContext.Clients
            .Group($"conversation-{conversationId}")
            .TypingIndicator(indicator);
    }

    public async Task BroadcastUserJoinedAsync(
        Guid conversationId,
        string userId,
        CancellationToken ct = default)
    {
        await hubContext.Clients
            .Group($"conversation-{conversationId}")
            .UserJoinedConversation(conversationId, Guid.Parse(userId), null);
    }

    public async Task BroadcastUserLeftAsync(
        Guid conversationId,
        string userId,
        CancellationToken ct = default)
    {
        await hubContext.Clients
            .Group($"conversation-{conversationId}")
            .UserLeftConversation(conversationId, Guid.Parse(userId));
    }
}
