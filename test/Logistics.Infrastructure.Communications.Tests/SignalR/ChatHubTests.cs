using System.Security.Claims;
using Logistics.Application.Abstractions.Realtime;
using Logistics.Infrastructure.Communications.SignalR.Clients;
using Logistics.Infrastructure.Communications.SignalR.Hubs;
using Logistics.Shared.Identity.Claims;
using Logistics.Shared.Models.Messaging;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.Communications.Tests.SignalR;

public class ChatHubTests
{
    private const string ConnectionId = "conn-1";

    private readonly IConversationAccess conversationAccess = Substitute.For<IConversationAccess>();
    private readonly ChatHubContext hubContext = new();
    private readonly IChatHubClient groupClient = Substitute.For<IChatHubClient>();
    private readonly IChatHubClient groupExceptClient = Substitute.For<IChatHubClient>();
    private readonly IGroupManager groups = Substitute.For<IGroupManager>();

    private readonly Guid callerTenantId = Guid.NewGuid();
    private readonly Guid callerUserId = Guid.NewGuid();
    private readonly Guid conversationId = Guid.NewGuid();

    private readonly ChatHub sut;

    public ChatHubTests()
    {
        sut = new ChatHub(hubContext, conversationAccess);

        var clients = Substitute.For<IHubCallerClients<IChatHubClient>>();
        clients.Group(Arg.Any<string>()).Returns(groupClient);
        clients.GroupExcept(Arg.Any<string>(), Arg.Any<IReadOnlyList<string>>()).Returns(groupExceptClient);

        sut.Clients = clients;
        sut.Groups = groups;
        sut.Context = CallerContext(callerTenantId, callerUserId);

        // Model a real connected client. Without this the pre-fix hub bailed out of the broadcast
        // methods for the wrong reason (no registered connection), which would have let the
        // negative tests below pass against the unfixed code.
        hubContext.AddClient(ConnectionId);
        hubContext.SetUserId(ConnectionId, callerUserId);
    }

    /// <summary>Passes the real join check, which is what authorizes the other methods.</summary>
    private async Task JoinAsync()
    {
        AllowJoin(true);
        await sut.JoinConversation(conversationId.ToString());
        conversationAccess.ClearReceivedCalls();
    }

    private void AllowJoin(bool allowed) =>
        conversationAccess.CanUserJoinConversationAsync(
                callerTenantId, conversationId, callerUserId, Arg.Any<CancellationToken>())
            .Returns(allowed);

    private static HubCallerContext CallerContext(Guid tenantId, Guid userId)
    {
        var context = Substitute.For<HubCallerContext>();
        context.ConnectionId.Returns(ConnectionId);
        context.User.Returns(new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(CustomClaimTypes.Tenant, tenantId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        ])));
        return context;
    }

    /// <summary>
    /// The bypass this guards: any authenticated user could previously join any conversation group
    /// by id, including one belonging to another tenant, and receive every message broadcast to it.
    /// </summary>
    [Fact]
    public async Task JoinConversation_CallerIsNotAParticipant_DoesNotJoinTheGroup()
    {
        AllowJoin(false);

        await sut.JoinConversation(conversationId.ToString());

        await groups.DidNotReceive().AddToGroupAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await groupClient.DidNotReceive().UserJoinedConversation(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>());
    }

    [Fact]
    public async Task JoinConversation_CallerIsAParticipant_JoinsTheGroupAndAnnounces()
    {
        AllowJoin(true);

        await sut.JoinConversation(conversationId.ToString());

        await groups.Received(1).AddToGroupAsync(
            ConnectionId, $"conversation-{conversationId}", Arg.Any<CancellationToken>());
        await groupClient.Received(1).UserJoinedConversation(conversationId, callerUserId, null);
    }

    /// <summary>
    /// The authorization check is asked about the caller's own claimed tenant and user, never
    /// about anything the client supplied, so a caller cannot ask on someone else's behalf.
    /// </summary>
    [Fact]
    public async Task JoinConversation_ChecksAccessForTheCallersOwnTenantAndUser()
    {
        AllowJoin(true);

        await sut.JoinConversation(conversationId.ToString());

        await conversationAccess.Received(1).CanUserJoinConversationAsync(
            callerTenantId, conversationId, callerUserId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task JoinConversation_WithoutATenantClaim_DoesNothing()
    {
        var context = Substitute.For<HubCallerContext>();
        context.ConnectionId.Returns(ConnectionId);
        context.User.Returns(new ClaimsPrincipal(new ClaimsIdentity()));
        sut.Context = context;

        await sut.JoinConversation(conversationId.ToString());

        await conversationAccess.DidNotReceive().CanUserJoinConversationAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await groups.DidNotReceive().AddToGroupAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// A non-GUID id previously reached <c>Guid.Parse</c> after the connection had already been
    /// added to the group, so the caller kept a subscription the throw never undid.
    /// </summary>
    [Fact]
    public async Task JoinConversation_MalformedConversationId_DoesNotThrowOrJoin()
    {
        await sut.JoinConversation("not-a-guid");

        await groups.DidNotReceive().AddToGroupAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// The group name is built from the parsed <see cref="Guid"/>, not the caller's spelling of it.
    /// Echoing the raw string put a caller who sent a braced or upper-case id into a group the
    /// server's own broadcasts — which always format canonically — never target, so the connection
    /// joined successfully and then silently received nothing.
    /// </summary>
    [Fact]
    public async Task JoinConversation_NonCanonicalConversationId_JoinsTheCanonicalGroup()
    {
        AllowJoin(true);

        await sut.JoinConversation(conversationId.ToString("B"));

        await groups.Received(1).AddToGroupAsync(
            ConnectionId, $"conversation-{conversationId}", Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Read receipts were broadcast into any conversation group by id, with no check at all.
    /// </summary>
    [Fact]
    public async Task MarkAsRead_WithoutHavingJoined_DoesNotBroadcast()
    {
        await sut.MarkAsRead(conversationId, Guid.NewGuid(), callerUserId);

        await groupClient.DidNotReceive().MessageRead(Arg.Any<Guid>(), Arg.Any<Guid>());
    }

    /// <summary>
    /// Trusting the supplied reader id let any caller pin a read on somebody else.
    /// </summary>
    [Fact]
    public async Task MarkAsRead_AttributesTheReceiptToTheCallerNotTheSuppliedReaderId()
    {
        await JoinAsync();
        var messageId = Guid.NewGuid();
        var someoneElse = Guid.NewGuid();

        await sut.MarkAsRead(conversationId, messageId, someoneElse);

        await groupClient.Received(1).MessageRead(messageId, callerUserId);
        await groupClient.DidNotReceive().MessageRead(messageId, someoneElse);
    }

    /// <summary>
    /// Typing indicators were broadcast into any conversation group by id, leaking presence into
    /// another tenant's thread and letting a caller impersonate one.
    /// </summary>
    [Fact]
    public async Task SendTypingIndicator_WithoutHavingJoined_DoesNotBroadcast()
    {
        await sut.SendTypingIndicator(conversationId.ToString(), true);

        await groupExceptClient.DidNotReceive().TypingIndicator(Arg.Any<TypingIndicatorDto>());
    }

    [Fact]
    public async Task SendTypingIndicator_AfterJoining_BroadcastsWithTheCallersOwnIdentity()
    {
        await JoinAsync();

        await sut.SendTypingIndicator(conversationId.ToString(), true);

        await groupExceptClient.Received(1).TypingIndicator(Arg.Is<TypingIndicatorDto>(
            i => i.ConversationId == conversationId && i.UserId == callerUserId && i.IsTyping));
    }

    [Fact]
    public async Task SendTypingIndicator_MalformedConversationId_DoesNotThrowOrBroadcast()
    {
        await JoinAsync();

        await sut.SendTypingIndicator("not-a-guid", true);

        await groupExceptClient.DidNotReceive().TypingIndicator(Arg.Any<TypingIndicatorDto>());
    }

    /// <summary>
    /// The web client fires a typing indicator on every keystroke, so this must not query.
    /// </summary>
    [Fact]
    public async Task SendTypingIndicator_DoesNotHitTheDatabase()
    {
        await JoinAsync();

        await sut.SendTypingIndicator(conversationId.ToString(), true);
        await sut.SendTypingIndicator(conversationId.ToString(), true);

        await conversationAccess.DidNotReceive().CanUserJoinConversationAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Dropping your own connection is always safe and stays ungated; announcing the departure to
    /// the conversation is a broadcast, so it does not. A caller could previously spoof a "user
    /// left" event into any conversation they had never joined.
    /// </summary>
    [Fact]
    public async Task LeaveConversation_WithoutHavingJoined_StillLeavesButDoesNotAnnounce()
    {
        await sut.LeaveConversation(conversationId.ToString());

        await groups.Received(1).RemoveFromGroupAsync(
            ConnectionId, $"conversation-{conversationId}", Arg.Any<CancellationToken>());
        await groupClient.DidNotReceive().UserLeftConversation(Arg.Any<Guid>(), Arg.Any<Guid>());
    }

    [Fact]
    public async Task LeaveConversation_AfterJoining_LeavesAndAnnounces()
    {
        await JoinAsync();

        await sut.LeaveConversation(conversationId.ToString());

        await groups.Received(1).RemoveFromGroupAsync(
            ConnectionId, $"conversation-{conversationId}", Arg.Any<CancellationToken>());
        await groupClient.Received(1).UserLeftConversation(conversationId, callerUserId);
    }

    /// <summary>
    /// Leaving drops the authorization, so a later broadcast has to rejoin to earn it back.
    /// </summary>
    [Fact]
    public async Task LeaveConversation_ThenTyping_DoesNotBroadcast()
    {
        await JoinAsync();
        await sut.LeaveConversation(conversationId.ToString());

        await sut.SendTypingIndicator(conversationId.ToString(), true);

        await groupExceptClient.DidNotReceive().TypingIndicator(Arg.Any<TypingIndicatorDto>());
    }

    /// <summary>
    /// A non-GUID id previously reached <c>Guid.Parse</c> inside the announcement, throwing out of
    /// the hub method before the connection was ever removed from the group.
    /// </summary>
    [Fact]
    public async Task LeaveConversation_MalformedConversationId_DoesNotThrow()
    {
        await sut.LeaveConversation("not-a-guid");

        await groups.DidNotReceive().RemoveFromGroupAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
