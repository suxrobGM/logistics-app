using Logistics.Application.Modules.Integrations.AICopilot.Commands;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Primitives.Enums;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.AICopilot;

public class DeleteAICopilotConversationHandlerTests
{
    private readonly AgentTestContext ctx = new();
    private readonly DeleteAICopilotConversationHandler sut;

    public DeleteAICopilotConversationHandlerTests()
    {
        sut = new DeleteAICopilotConversationHandler(ctx.TenantUow, ctx.CurrentUser);
    }

    [Fact]
    public async Task Handle_ConversationNotFound_Fails()
    {
        var result = await sut.Handle(
            new DeleteAICopilotConversationCommand { ConversationId = Guid.NewGuid() }, CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_NotOwner_Fails()
    {
        var conversation = ctx.SetConversation(createdById: Guid.NewGuid(), kind: AgentConversationKind.Copilot);

        var result = await sut.Handle(
            new DeleteAICopilotConversationCommand { ConversationId = conversation.Id }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        ctx.ConversationRepo.DidNotReceiveWithAnyArgs().Delete(default);
    }

    [Fact]
    public async Task Handle_DispatchKindConversation_Fails()
    {
        var conversation = ctx.SetConversation(kind: AgentConversationKind.Dispatch);

        var result = await sut.Handle(
            new DeleteAICopilotConversationCommand { ConversationId = conversation.Id }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        ctx.ConversationRepo.DidNotReceiveWithAnyArgs().Delete(default);
    }

    [Fact]
    public async Task Handle_TurnRunning_Fails()
    {
        var conversation = ctx.SetConversation(kind: AgentConversationKind.Copilot);
        conversation.BeginTurn();

        var result = await sut.Handle(
            new DeleteAICopilotConversationCommand { ConversationId = conversation.Id }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("running", result.Error);
        ctx.ConversationRepo.DidNotReceiveWithAnyArgs().Delete(default);
    }

    [Fact]
    public async Task Handle_Success_DeletesAndSaves()
    {
        var conversation = ctx.SetConversation(kind: AgentConversationKind.Copilot);

        var result = await sut.Handle(
            new DeleteAICopilotConversationCommand { ConversationId = conversation.Id }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        ctx.ConversationRepo.Received(1).Delete(conversation);
        await ctx.TenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
