using Logistics.Application.Modules.Integrations.AICopilot.Queries;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.AICopilot;

public class GetAICopilotConversationsHandlerTests
{
    private readonly AgentTestContext _ctx = new();
    private readonly GetAICopilotConversationsHandler _sut;

    public GetAICopilotConversationsHandlerTests()
    {
        _sut = new GetAICopilotConversationsHandler(_ctx.Queries, _ctx.CurrentUser);
    }

    /// <summary>
    /// A dispatch conversation owned by the same user must never surface on the
    /// copilot list - only Kind == Copilot rows count.
    /// </summary>
    [Fact]
    public async Task Handle_OwnerHasDispatchConversation_OnlyCopilotKindReturned()
    {
        var copilotConversation = new AgentConversation { CreatedById = _ctx.UserId, Kind = AgentConversationKind.Copilot };
        var dispatchConversation = new AgentConversation { CreatedById = _ctx.UserId, Kind = AgentConversationKind.Dispatch };
        _ctx.ConversationRepo.Query().Returns(new List<AgentConversation> { copilotConversation, dispatchConversation }.BuildMock());

        var result = await _sut.Handle(new GetAICopilotConversationsQuery { Page = 1, PageSize = 20 }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value!);
        Assert.Equal(copilotConversation.Id, item.Id);
    }

    [Fact]
    public async Task Handle_OtherUsersConversation_NotReturned()
    {
        var other = new AgentConversation { CreatedById = Guid.NewGuid(), Kind = AgentConversationKind.Copilot };
        _ctx.ConversationRepo.Query().Returns(new List<AgentConversation> { other }.BuildMock());

        var result = await _sut.Handle(new GetAICopilotConversationsQuery { Page = 1, PageSize = 20 }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }
}
