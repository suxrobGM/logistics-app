using Logistics.Infrastructure.Communications.SignalR.Clients;
using Logistics.Infrastructure.Communications.SignalR.Hubs;
using Logistics.Shared.Models.Messaging;
using Microsoft.AspNetCore.SignalR;
using Logistics.Application.Abstractions.Realtime;

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

    public async Task BroadcastConversationReadAsync(
        Guid conversationId,
        Guid readById,
        CancellationToken ct = default)
    {
        await hubContext.Clients
            .Group($"conversation-{conversationId}")
            .ConversationRead(conversationId, readById);
    }
}
