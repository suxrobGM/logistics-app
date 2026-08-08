using Logistics.Application.Modules.Integrations.AIDispatch.Commands;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Primitives.Enums;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.AIDispatch;

public class RenameAIDispatchConversationHandlerTests
{
    private readonly AgentTestContext ctx = new();
    private readonly RenameAIDispatchConversationHandler sut;

    public RenameAIDispatchConversationHandlerTests()
    {
        sut = new RenameAIDispatchConversationHandler(ctx.TenantUow);
    }

    [Fact]
    public async Task Handle_ConversationNotFound_Fails()
    {
        var result = await sut.Handle(
            new RenameAIDispatchConversationCommand { ConversationId = Guid.NewGuid(), Title = "New title" },
            CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_CopilotKindConversation_Fails()
    {
        var conversation = ctx.SetConversation(kind: AgentConversationKind.Copilot);

        var result = await sut.Handle(
            new RenameAIDispatchConversationCommand { ConversationId = conversation.Id, Title = "New title" },
            CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_Success_TrimsTitleAndSaves()
    {
        var conversation = ctx.SetConversation(kind: AgentConversationKind.Dispatch);

        var result = await sut.Handle(
            new RenameAIDispatchConversationCommand { ConversationId = conversation.Id, Title = "  Plan the week  " },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Plan the week", conversation.Title);
        await ctx.TenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
