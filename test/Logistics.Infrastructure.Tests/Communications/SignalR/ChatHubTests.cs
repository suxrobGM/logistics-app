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

    private readonly IConversationAccess _conversationAccess = Substitute.For<IConversationAccess>();
    private readonly IChatHubClient _groupClient = Substitute.For<IChatHubClient>();
    private readonly IChatHubClient _groupExceptClient = Substitute.For<IChatHubClient>();
    private readonly IGroupManager _groups = Substitute.For<IGroupManager>();

    private readonly Guid _callerTenantId = Guid.NewGuid();
    private readonly Guid _callerUserId = Guid.NewGuid();
    private readonly Guid _conversationId = Guid.NewGuid();

    private readonly ChatHub _sut;

    public ChatHubTests()
    {
        _sut = new ChatHub(_conversationAccess);

        var clients = Substitute.For<IHubCallerClients<IChatHubClient>>();
        clients.Group(Arg.Any<string>()).Returns(_groupClient);
        clients.GroupExcept(Arg.Any<string>(), Arg.Any<IReadOnlyList<string>>()).Returns(_groupExceptClient);

        _sut.Clients = clients;
        _sut.Groups = _groups;
        _sut.Context = HubTestContext.Caller(_callerTenantId, _callerUserId);
    }

    /// <summary>Passes the real join check, which authorizes the other methods.</summary>
    private async Task JoinAsync()
    {
        AllowJoin(true);
        await _sut.JoinConversation(_conversationId.ToString());
    }

    private void AllowJoin(bool allowed) =>
        _conversationAccess.CanUserJoinConversationAsync(
                _callerTenantId, _conversationId, _callerUserId, Arg.Any<CancellationToken>())
            .Returns(allowed);

    // Any authenticated user could previously join any conversation group by id.
    [Fact]
    public async Task JoinConversation_CallerIsNotAParticipant_DoesNotJoinTheGroup()
    {
        AllowJoin(false);

        await _sut.JoinConversation(_conversationId.ToString());

        await _groups.DidNotReceive().AddToGroupAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _groupClient.DidNotReceive().UserJoinedConversation(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>());
    }

    // The check is asked about the caller's own claims, so this only passes for matching arguments.
    [Fact]
    public async Task JoinConversation_CallerIsAParticipant_JoinsTheGroupAndAnnounces()
    {
        AllowJoin(true);

        await _sut.JoinConversation(_conversationId.ToString());

        await _groups.Received(1).AddToGroupAsync(
            ConnectionId, $"conversation-{_conversationId}", Arg.Any<CancellationToken>());
        await _groupClient.Received(1).UserJoinedConversation(_conversationId, _callerUserId, null);
    }

    // A non-GUID id previously reached Guid.Parse after the group add, leaving the caller joined.
    [Fact]
    public async Task JoinConversation_MalformedConversationId_DoesNotThrowOrJoin()
    {
        await _sut.JoinConversation("not-a-guid");

        await _groups.DidNotReceive().AddToGroupAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    // Broadcasts format the group name from a Guid, so a raw string joins a group nothing targets.
    [Fact]
    public async Task JoinConversation_NonCanonicalConversationId_JoinsTheCanonicalGroup()
    {
        AllowJoin(true);

        await _sut.JoinConversation(_conversationId.ToString("B"));

        await _groups.Received(1).AddToGroupAsync(
            ConnectionId, $"conversation-{_conversationId}", Arg.Any<CancellationToken>());
    }

    // Typing indicators were broadcast into any conversation group by id.
    [Fact]
    public async Task SendTypingIndicator_WithoutHavingJoined_DoesNotBroadcast()
    {
        await _sut.SendTypingIndicator(_conversationId.ToString(), true);

        await _groupExceptClient.DidNotReceive().TypingIndicator(Arg.Any<TypingIndicatorDto>());
    }

    [Fact]
    public async Task SendTypingIndicator_AfterJoining_BroadcastsWithTheCallersOwnIdentity()
    {
        await JoinAsync();

        await _sut.SendTypingIndicator(_conversationId.ToString(), true);

        await _groupExceptClient.Received(1).TypingIndicator(Arg.Is<TypingIndicatorDto>(
            i => i.ConversationId == _conversationId && i.UserId == _callerUserId && i.IsTyping));
    }

    // A caller could previously spoof a "user left" event into a conversation never joined.
    [Fact]
    public async Task LeaveConversation_WithoutHavingJoined_StillLeavesButDoesNotAnnounce()
    {
        await _sut.LeaveConversation(_conversationId.ToString());

        await _groups.Received(1).RemoveFromGroupAsync(
            ConnectionId, $"conversation-{_conversationId}", Arg.Any<CancellationToken>());
        await _groupClient.DidNotReceive().UserLeftConversation(Arg.Any<Guid>(), Arg.Any<Guid>());
    }

    [Fact]
    public async Task LeaveConversation_AfterJoining_LeavesAndAnnounces()
    {
        await JoinAsync();

        await _sut.LeaveConversation(_conversationId.ToString());

        await _groups.Received(1).RemoveFromGroupAsync(
            ConnectionId, $"conversation-{_conversationId}", Arg.Any<CancellationToken>());
        await _groupClient.Received(1).UserLeftConversation(_conversationId, _callerUserId);
    }

    // Leaving drops the authorization, so a later broadcast must rejoin to earn it back.
    [Fact]
    public async Task LeaveConversation_ThenTyping_DoesNotBroadcast()
    {
        await JoinAsync();
        await _sut.LeaveConversation(_conversationId.ToString());

        await _sut.SendTypingIndicator(_conversationId.ToString(), true);

        await _groupExceptClient.DidNotReceive().TypingIndicator(Arg.Any<TypingIndicatorDto>());
    }
}
