using Logistics.Infrastructure.Communications.SignalR.Clients;
using Logistics.Shared.Models.Messaging;
using Microsoft.AspNetCore.SignalR;

namespace Logistics.Infrastructure.Communications.SignalR.Hubs;

/// <summary>
///     SignalR hub for real-time messaging between dispatchers and drivers.
/// </summary>
public class ChatHub(ChatHubContext hubContext) : Hub<IChatHubClient>
{
    public override Task OnConnectedAsync()
    {
        hubContext.AddClient(Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        hubContext.RemoveClient(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    ///     Register the connection with a tenant for multi-tenant message routing.
    /// </summary>
    public async Task RegisterTenant(string tenantId)
    {
        hubContext.SetTenantId(Context.ConnectionId, tenantId);
        await Groups.AddToGroupAsync(Context.ConnectionId, tenantId);
    }

    /// <summary>
    ///     Unregister from a tenant group.
    /// </summary>
    public async Task UnregisterTenant(string tenantId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, tenantId);
    }

    /// <summary>
    ///     Register the current user ID for the connection.
    /// </summary>
    public Task RegisterUser(Guid userId)
    {
        hubContext.SetUserId(Context.ConnectionId, userId);
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Join a conversation to receive messages.
    /// </summary>
    public async Task JoinConversation(string conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation-{conversationId}");

        var userId = hubContext.GetUserId(Context.ConnectionId);
        if (userId.HasValue)
        {
            await Clients.Group($"conversation-{conversationId}")
                .UserJoinedConversation(Guid.Parse(conversationId), userId.Value, null);
        }
    }

    /// <summary>
    ///     Leave a conversation.
    /// </summary>
    public async Task LeaveConversation(string conversationId)
    {
        var userId = hubContext.GetUserId(Context.ConnectionId);
        if (userId.HasValue)
        {
            await Clients.Group($"conversation-{conversationId}")
                .UserLeftConversation(Guid.Parse(conversationId), userId.Value);
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation-{conversationId}");
    }

    /// <summary>
    ///     Send a message to a conversation.
    ///     Messages are persisted via the API and then broadcast here.
    /// </summary>
    public async Task SendMessage(MessageDto message)
    {
        await Clients.Group($"conversation-{message.ConversationId}")
            .ReceiveMessage(message);
    }

    /// <summary>
    ///     Notify that a message has been read.
    /// </summary>
    public async Task MarkAsRead(Guid conversationId, Guid messageId, Guid readById)
    {
        await Clients.Group($"conversation-{conversationId}")
            .MessageRead(messageId, readById);
    }

    /// <summary>
    ///     Send typing indicator to a conversation.
    /// </summary>
    public async Task SendTypingIndicator(string conversationId, bool isTyping)
    {
        var userId = hubContext.GetUserId(Context.ConnectionId);
        if (!userId.HasValue)
        {
            return;
        }

        var indicator = new TypingIndicatorDto
        {
            ConversationId = Guid.Parse(conversationId), UserId = userId.Value, IsTyping = isTyping
        };

        await Clients.GroupExcept($"conversation-{conversationId}", Context.ConnectionId)
            .TypingIndicator(indicator);
    }
}
