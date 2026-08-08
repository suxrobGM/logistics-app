using Logistics.Application.Modules.Integrations.AIDispatch.Commands;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Primitives.Enums;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.AIDispatch;

public class DeleteAIDispatchConversationHandlerTests
{
    private readonly AgentTestContext ctx = new();
    private readonly DeleteAIDispatchConversationHandler sut;

    public DeleteAIDispatchConversationHandlerTests()
    {
        sut = new DeleteAIDispatchConversationHandler(ctx.Commands);
    }

    [Fact]
    public async Task Handle_ConversationNotFound_Fails()
    {
        var result = await sut.Handle(
            new DeleteAIDispatchConversationCommand { ConversationId = Guid.NewGuid() }, CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_CopilotKindConversation_Fails()
    {
        var conversation = ctx.SetConversation(kind: AgentConversationKind.Copilot);

        var result = await sut.Handle(
            new DeleteAIDispatchConversationCommand { ConversationId = conversation.Id }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        ctx.ConversationRepo.DidNotReceiveWithAnyArgs().Delete(default);
    }

    [Fact]
    public async Task Handle_TurnRunning_Fails()
    {
        var conversation = ctx.SetConversation(kind: AgentConversationKind.Dispatch);
        conversation.BeginTurn();

        var result = await sut.Handle(
            new DeleteAIDispatchConversationCommand { ConversationId = conversation.Id }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("running", result.Error);
        ctx.ConversationRepo.DidNotReceiveWithAnyArgs().Delete(default);
    }

    [Fact]
    public async Task Handle_Success_DeletesAndSaves()
    {
        var conversation = ctx.SetConversation(kind: AgentConversationKind.Dispatch);

        var result = await sut.Handle(
            new DeleteAIDispatchConversationCommand { ConversationId = conversation.Id }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        ctx.ConversationRepo.Received(1).Delete(conversation);
        await ctx.TenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
