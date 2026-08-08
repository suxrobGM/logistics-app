using Logistics.Application.Modules.Integrations.AICopilot.Commands;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.AICopilot;

public class CreateAICopilotConversationHandlerTests
{
    private readonly AgentTestContext ctx = new();
    private readonly CreateAICopilotConversationHandler sut;

    public CreateAICopilotConversationHandlerTests()
    {
        sut = new CreateAICopilotConversationHandler(ctx.TenantUow, ctx.CurrentUser);
    }

    [Fact]
    public async Task Handle_NotAuthenticated_Fails()
    {
        ctx.CurrentUser.GetUserId().Returns((Guid?)null);

        var result = await sut.Handle(new CreateAICopilotConversationCommand(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        await ctx.ConversationRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task Handle_Success_CreatesConversationWithCopilotKind()
    {
        AgentConversation? added = null;
        await ctx.ConversationRepo.AddAsync(Arg.Do<AgentConversation>(c => added = c), Arg.Any<CancellationToken>());

        var result = await sut.Handle(new CreateAICopilotConversationCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(AgentConversationKind.Copilot, added!.Kind);
        Assert.Equal(ctx.UserId, added.CreatedById);
        await ctx.TenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
