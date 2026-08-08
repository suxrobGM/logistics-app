using Logistics.Application.Modules.Integrations.AIDispatch.Queries;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.AIDispatch;

public class GetAIDispatchConversationsHandlerTests
{
    private readonly AgentTestContext ctx = new();
    private readonly GetAIDispatchConversationsHandler sut;

    public GetAIDispatchConversationsHandlerTests()
    {
        sut = new GetAIDispatchConversationsHandler(ctx.Queries);
    }

    /// <summary>Dispatch conversations are tenant-shared - every user's rows come back, not just the caller's.</summary>
    [Fact]
    public async Task Handle_ReturnsOtherUsersDispatchConversations()
    {
        var mine = new AgentConversation { CreatedById = Guid.NewGuid(), Kind = AgentConversationKind.Dispatch };
        var someoneElses = new AgentConversation { CreatedById = Guid.NewGuid(), Kind = AgentConversationKind.Dispatch };
        ctx.ConversationRepo.Query().Returns(new List<AgentConversation> { mine, someoneElses }.BuildMock());

        var result = await sut.Handle(new GetAIDispatchConversationsQuery { Page = 1, PageSize = 20 }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count());
    }

    /// <summary>A copilot conversation must never leak into the tenant-shared dispatch list.</summary>
    [Fact]
    public async Task Handle_NeverReturnsCopilotKindConversations()
    {
        var dispatchConversation = new AgentConversation { CreatedById = Guid.NewGuid(), Kind = AgentConversationKind.Dispatch };
        var copilotConversation = new AgentConversation { CreatedById = Guid.NewGuid(), Kind = AgentConversationKind.Copilot };
        ctx.ConversationRepo.Query().Returns(new List<AgentConversation> { dispatchConversation, copilotConversation }.BuildMock());

        var result = await sut.Handle(new GetAIDispatchConversationsQuery { Page = 1, PageSize = 20 }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value!);
        Assert.Equal(dispatchConversation.Id, item.Id);
    }
}
