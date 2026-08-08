using Logistics.Application.Modules.Integrations.AIDispatch.Commands;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.AIDispatch;

public class CreateAIDispatchConversationHandlerTests
{
    private readonly AgentTestContext ctx = new();
    private readonly CreateAIDispatchConversationHandler sut;

    public CreateAIDispatchConversationHandlerTests()
    {
        sut = new CreateAIDispatchConversationHandler(ctx.Commands, ctx.CurrentUser);
    }

    [Fact]
    public async Task Handle_NotAuthenticated_Fails()
    {
        ctx.CurrentUser.GetUserId().Returns((Guid?)null);

        var result = await sut.Handle(new CreateAIDispatchConversationCommand(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        await ctx.ConversationRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task Handle_Success_CreatesTenantSharedConversationWithDispatchKind()
    {
        AgentConversation? added = null;
        await ctx.ConversationRepo.AddAsync(Arg.Do<AgentConversation>(c => added = c), Arg.Any<CancellationToken>());

        var result = await sut.Handle(new CreateAIDispatchConversationCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(AgentConversationKind.Dispatch, added!.Kind);
        Assert.Equal(ctx.UserId, added.CreatedById);
        await ctx.TenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
