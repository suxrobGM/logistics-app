using Logistics.Application.Abstractions.Realtime;
using Logistics.Infrastructure.Communications.SignalR.Clients;
using Logistics.Shared.Models.Messaging;
using Microsoft.AspNetCore.SignalR;

namespace Logistics.Infrastructure.Communications.SignalR.Hubs;

/// <summary>Provides tenant-scoped messaging between dispatchers and drivers.</summary>
public class ChatHub(ChatHubContext hubContext, IConversationAccess conversationAccess)
    : TenantHub<IChatHubClient>
{
    protected override Task OnTenantConnectedAsync(Guid tenantId, Guid userId)
    {
        hubContext.AddClient(Context.ConnectionId);
        hubContext.SetTenantId(Context.ConnectionId, tenantId.ToString());
        hubContext.SetUserId(Context.ConnectionId, userId);
        return Task.CompletedTask;
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        hubContext.RemoveClient(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    ///     Join a conversation to receive messages.
    /// </summary>
    public async Task JoinConversation(string conversationId)
    {
        if (Context.TenantIdFromClaim() is not { } tenantId ||
            Context.UserIdFromClaim() is not { } userId ||
            !Guid.TryParse(conversationId, out var conversationGuid))
        {
            return;
        }

        if (!await conversationAccess.CanUserJoinConversationAsync(tenantId, conversationGuid, userId))
        {
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(conversationGuid));
        hubContext.AddAuthorizedConversation(Context.ConnectionId, conversationGuid);

        await Clients.Group(GroupName(conversationGuid))
            .UserJoinedConversation(conversationGuid, userId, null);
    }

    /// <summary>
    ///     Leave a conversation.
    /// </summary>
    public async Task LeaveConversation(string conversationId)
    {
        if (!Guid.TryParse(conversationId, out var conversationGuid))
        {
            return;
        }

        // Dropping your own connection from a group is always safe, so it is not gated.
        // Announcing the departure to everyone in the conversation is a broadcast, so it is.
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(conversationGuid));

        if (Caller(conversationGuid) is { } userId)
        {
            await Clients.Group(GroupName(conversationGuid))
                .UserLeftConversation(conversationGuid, userId);
        }

        hubContext.RemoveAuthorizedConversation(Context.ConnectionId, conversationGuid);
    }

    /// <summary>
    ///     Notify that a message has been read.
    /// </summary>
    /// <param name="readById">
    ///     Ignored. Kept because the mobile client wrappers still send it. The receipt is always
    ///     attributed to the caller's own identity.
    /// </param>
    public async Task MarkAsRead(Guid conversationId, Guid messageId, Guid readById)
    {
        if (Caller(conversationId) is not { } userId)
        {
            return;
        }

        await Clients.Group(GroupName(conversationId)).MessageRead(messageId, userId);
    }

    /// <summary>
    ///     Send typing indicator to a conversation.
    /// </summary>
    public async Task SendTypingIndicator(string conversationId, bool isTyping)
    {
        if (!Guid.TryParse(conversationId, out var conversationGuid) ||
            Caller(conversationGuid) is not { } userId)
        {
            return;
        }

        var indicator = new TypingIndicatorDto
        {
            ConversationId = conversationGuid, UserId = userId, IsTyping = isTyping
        };

        await Clients.GroupExcept(GroupName(conversationGuid), Context.ConnectionId)
            .TypingIndicator(indicator);
    }

    /// <summary>
    ///     The caller's user id if this connection may broadcast into the conversation, else null.
    ///     Authorization is established once by <see cref="JoinConversation" /> and cached against the
    ///     connection, so it lives exactly as long as the group membership it guards: a reconnect
    ///     drops both and the client rejoins. Keeps typing indicators off the database.
    /// </summary>
    private Guid? Caller(Guid conversationId)
    {
        return hubContext.IsAuthorizedForConversation(Context.ConnectionId, conversationId)
            ? Context.UserIdFromClaim()
            : null;
    }

    /// <summary>
    ///     Built from the parsed Guid, not the caller's string, so a braced or upper-case id cannot
    ///     land the caller in a group the server's own broadcasts never target.
    /// </summary>
    private static string GroupName(Guid conversationId) => $"conversation-{conversationId}";
}
