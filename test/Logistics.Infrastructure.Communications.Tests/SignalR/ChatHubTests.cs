using Logistics.Application.Abstractions.Realtime;
using Logistics.Infrastructure.Communications.SignalR.Clients;
using Logistics.Infrastructure.Communications.SignalR.Hubs;
using Logistics.Shared.Models.Messaging;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.Communications.Tests.SignalR;

public class ChatHubTests
{
    private const string ConnectionId = HubTestContext.ConnectionId;

    private readonly IConversationAccess conversationAccess = Substitute.For<IConversationAccess>();
    private readonly IChatHubClient groupClient = Substitute.For<IChatHubClient>();
    private readonly IChatHubClient groupExceptClient = Substitute.For<IChatHubClient>();
    private readonly IGroupManager groups = Substitute.For<IGroupManager>();

    private readonly Guid callerTenantId = Guid.NewGuid();
    private readonly Guid callerUserId = Guid.NewGuid();
    private readonly Guid conversationId = Guid.NewGuid();

    private readonly ChatHub sut;

    public ChatHubTests()
    {
        sut = new ChatHub(conversationAccess);

        var clients = Substitute.For<IHubCallerClients<IChatHubClient>>();
        clients.Group(Arg.Any<string>()).Returns(groupClient);
        clients.GroupExcept(Arg.Any<string>(), Arg.Any<IReadOnlyList<string>>()).Returns(groupExceptClient);

        sut.Clients = clients;
        sut.Groups = groups;
        sut.Context = HubTestContext.Caller(callerTenantId, callerUserId);
    }

    /// <summary>Passes the real join check, which authorizes the other methods.</summary>
    private async Task JoinAsync()
    {
        AllowJoin(true);
        await sut.JoinConversation(conversationId.ToString());
    }

    private void AllowJoin(bool allowed) =>
        conversationAccess.CanUserJoinConversationAsync(
                callerTenantId, conversationId, callerUserId, Arg.Any<CancellationToken>())
            .Returns(allowed);

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

    // The check is asked about the caller's own claims, so this only passes for matching arguments.
    [Fact]
    public async Task JoinConversation_CallerIsAParticipant_JoinsTheGroupAndAnnounces()
    {
        AllowJoin(true);

        await sut.JoinConversation(conversationId.ToString());

        await groups.Received(1).AddToGroupAsync(
            ConnectionId, $"conversation-{conversationId}", Arg.Any<CancellationToken>());
        await groupClient.Received(1).UserJoinedConversation(conversationId, callerUserId, null);
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
}
