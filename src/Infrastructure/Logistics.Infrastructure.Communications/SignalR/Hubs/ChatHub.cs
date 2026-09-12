using Logistics.Application.Abstractions.Realtime;
using Logistics.Infrastructure.Communications.SignalR.Clients;
using Logistics.Shared.Models.Messaging;
using Microsoft.AspNetCore.SignalR;

namespace Logistics.Infrastructure.Communications.SignalR.Hubs;

/// <summary>Provides tenant-scoped messaging between dispatchers and drivers.</summary>
public class ChatHub(IConversationAccess conversationAccess) : TenantHub<IChatHubClient>
{
    /// <summary>
    ///     Join a conversation to receive messages. A caller who is not a participant is not added.
    /// </summary>
    public async Task JoinConversation(string conversationId)
    {
        if (!Guid.TryParse(conversationId, out var conversationGuid))
        {
            return;
        }

        if (!await conversationAccess.CanUserJoinConversationAsync(TenantId, conversationGuid, UserId))
        {
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(conversationGuid));
        Context.Items[JoinedKey(conversationGuid)] = true;

        await Clients.Group(GroupName(conversationGuid))
            .UserJoinedConversation(conversationGuid, UserId, null);
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

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(conversationGuid));

        // Announcing a departure is a broadcast, so only a caller who actually joined may make it.
        if (HasJoined(conversationGuid))
        {
            await Clients.Group(GroupName(conversationGuid))
                .UserLeftConversation(conversationGuid, UserId);
        }

        Context.Items.Remove(JoinedKey(conversationGuid));
    }

    /// <summary>
    ///     Send typing indicator to a conversation.
    /// </summary>
    public async Task SendTypingIndicator(string conversationId, bool isTyping)
    {
        if (!Guid.TryParse(conversationId, out var conversationGuid) || !HasJoined(conversationGuid))
        {
            return;
        }

        var indicator = new TypingIndicatorDto
        {
            ConversationId = conversationGuid, UserId = UserId, IsTyping = isTyping
        };

        await Clients.GroupExcept(GroupName(conversationGuid), Context.ConnectionId)
            .TypingIndicator(indicator);
    }

    /// <summary>
    ///     Whether this connection passed the <see cref="JoinConversation" /> check. The flag lives on
    ///     the connection, so it lasts exactly as long as the group membership it guards.
    /// </summary>
    private bool HasJoined(Guid conversationId) => Context.Items.ContainsKey(JoinedKey(conversationId));

    private static string JoinedKey(Guid conversationId) => $"joined-conversation-{conversationId}";

    /// <summary>
    ///     Built from the parsed Guid, not the caller's string, so a braced or upper-case id cannot
    ///     land the caller in a group the server never broadcasts to.
    /// </summary>
    private static string GroupName(Guid conversationId) => $"conversation-{conversationId}";
}
