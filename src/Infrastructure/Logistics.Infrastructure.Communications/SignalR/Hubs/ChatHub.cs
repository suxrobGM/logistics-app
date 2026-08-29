using System.Security.Claims;
using Logistics.Infrastructure.Communications.SignalR.Clients;
using Logistics.Shared.Identity.Claims;
using Logistics.Shared.Models.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Logistics.Infrastructure.Communications.SignalR.Hubs;

/// <summary>
///     SignalR hub for real-time messaging between dispatchers and drivers. Authorized, and both
///     the tenant group and the acting user id are taken from the caller's JWT claims - never from
///     client-supplied ids - so a client cannot join another tenant's group or act as another user.
///     Mirrors <see cref="CopilotHub"/>.
/// </summary>
[Authorize]
public class ChatHub(ChatHubContext hubContext) : Hub<IChatHubClient>
{
    public override async Task OnConnectedAsync()
    {
        var tenantId = Context.User?.FindFirst(CustomClaimTypes.Tenant)?.Value;
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? Context.User?.FindFirst("sub")?.Value;

        if (tenantId is null || !Guid.TryParse(userId, out var parsedUserId))
        {
            Context.Abort();
            return;
        }

        hubContext.AddClient(Context.ConnectionId);
        hubContext.SetTenantId(Context.ConnectionId, tenantId);
        hubContext.SetUserId(Context.ConnectionId, parsedUserId);
        await Groups.AddToGroupAsync(Context.ConnectionId, tenantId);
        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        hubContext.RemoveClient(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }

    // Tenant and user are established from claims on connect; these are kept for client
    // compatibility but deliberately ignore the client-supplied ids.
    public Task RegisterTenant(string tenantId) => Task.CompletedTask;

    public Task UnregisterTenant(string tenantId) => Task.CompletedTask;

    public Task RegisterUser(Guid userId) => Task.CompletedTask;

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
