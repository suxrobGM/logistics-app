using System.Security.Claims;
using Logistics.Application.Abstractions.Realtime;
using Logistics.Infrastructure.Communications.SignalR.Clients;
using Logistics.Infrastructure.Communications.SignalR.Hubs;
using Logistics.Shared.Identity.Claims;
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

        sut.Clients = clients;
        sut.Groups = groups;
        sut.Context = CallerContext(callerTenantId, callerUserId);
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
}
