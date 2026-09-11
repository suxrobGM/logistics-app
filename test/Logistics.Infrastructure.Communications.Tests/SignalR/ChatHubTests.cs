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

        // Model a real connected client. Without it the negative tests below would pass against
        // the unfixed hub for the wrong reason.
        hubContext.AddClient(ConnectionId);
        hubContext.SetUserId(ConnectionId, callerUserId);
    }

    /// <summary>Passes the real join check, which authorizes the other methods.</summary>
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

    // Any authenticated user could previously join any conversation group by id.
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

    // The check is asked about the caller's own claims, never about client-supplied values.
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

    // A non-GUID id previously reached Guid.Parse after the group add, leaving the caller joined.
    [Fact]
    public async Task JoinConversation_MalformedConversationId_DoesNotThrowOrJoin()
    {
        await sut.JoinConversation("not-a-guid");

        await groups.DidNotReceive().AddToGroupAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    // Broadcasts format the group name from a Guid, so a raw string joins a group nothing targets.
    [Fact]
    public async Task JoinConversation_NonCanonicalConversationId_JoinsTheCanonicalGroup()
    {
        AllowJoin(true);

        await sut.JoinConversation(conversationId.ToString("B"));

        await groups.Received(1).AddToGroupAsync(
            ConnectionId, $"conversation-{conversationId}", Arg.Any<CancellationToken>());
    }

    // Read receipts were broadcast into any conversation group by id, with no check.
    [Fact]
    public async Task MarkAsRead_WithoutHavingJoined_DoesNotBroadcast()
    {
        await sut.MarkAsRead(conversationId, Guid.NewGuid(), callerUserId);

        await groupClient.DidNotReceive().MessageRead(Arg.Any<Guid>(), Arg.Any<Guid>());
    }

    // Trusting the supplied reader id let any caller pin a read on somebody else.
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

    // Typing indicators were broadcast into any conversation group by id.
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

    // The web client fires a typing indicator on every keystroke, so this must not query.
    [Fact]
    public async Task SendTypingIndicator_DoesNotHitTheDatabase()
    {
        await JoinAsync();

        await sut.SendTypingIndicator(conversationId.ToString(), true);
        await sut.SendTypingIndicator(conversationId.ToString(), true);

        await conversationAccess.DidNotReceive().CanUserJoinConversationAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    // A caller could previously spoof a "user left" event into a conversation never joined.
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

    // Leaving drops the authorization, so a later broadcast must rejoin to earn it back.
    [Fact]
    public async Task LeaveConversation_ThenTyping_DoesNotBroadcast()
    {
        await JoinAsync();
        await sut.LeaveConversation(conversationId.ToString());

        await sut.SendTypingIndicator(conversationId.ToString(), true);

        await groupExceptClient.DidNotReceive().TypingIndicator(Arg.Any<TypingIndicatorDto>());
    }

    // A non-GUID id previously threw inside the announcement, before the group removal ran.
    [Fact]
    public async Task LeaveConversation_MalformedConversationId_DoesNotThrow()
    {
        await sut.LeaveConversation("not-a-guid");

        await groups.DidNotReceive().RemoveFromGroupAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
