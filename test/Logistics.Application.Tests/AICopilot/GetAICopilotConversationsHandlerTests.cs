using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Integrations.AICopilot.Queries;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.AICopilot;

public class GetAICopilotConversationsHandlerTests
{
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ICurrentUserService currentUser = Substitute.For<ICurrentUserService>();
    private readonly ITenantRepository<AgentConversation, Guid> conversationRepo =
        Substitute.For<ITenantRepository<AgentConversation, Guid>>();

    private readonly Guid userId = Guid.NewGuid();
    private readonly GetAICopilotConversationsHandler sut;

    public GetAICopilotConversationsHandlerTests()
    {
        tenantUow.Repository<AgentConversation>().Returns(conversationRepo);
        currentUser.GetUserId().Returns(userId);
        sut = new GetAICopilotConversationsHandler(tenantUow, currentUser);
    }

    /// <summary>
    /// A future Phase 3 dispatch conversation owned by the same user must never surface on the
    /// copilot list - only Kind == Copilot rows count.
    /// </summary>
    [Fact]
    public async Task Handle_OwnerHasDispatchConversation_OnlyCopilotKindReturned()
    {
        var copilotConversation = new AgentConversation { CreatedById = userId, Kind = AgentConversationKind.Copilot };
        var dispatchConversation = new AgentConversation { CreatedById = userId, Kind = AgentConversationKind.Dispatch };
        conversationRepo.Query().Returns(new List<AgentConversation> { copilotConversation, dispatchConversation }.BuildMock());

        var result = await sut.Handle(new GetAICopilotConversationsQuery { Page = 1, PageSize = 20 }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value!);
        Assert.Equal(copilotConversation.Id, item.Id);
    }

    [Fact]
    public async Task Handle_OtherUsersConversation_NotReturned()
    {
        var other = new AgentConversation { CreatedById = Guid.NewGuid(), Kind = AgentConversationKind.Copilot };
        conversationRepo.Query().Returns(new List<AgentConversation> { other }.BuildMock());

        var result = await sut.Handle(new GetAICopilotConversationsQuery { Page = 1, PageSize = 20 }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }
}
