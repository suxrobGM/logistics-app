using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Modules.Integrations.AIDispatch.Commands;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.AIDispatch;

public class CancelAIDispatchTurnHandlerTests
{
    private readonly AgentTestContext ctx = new();
    private readonly IAIDispatchService dispatchService = Substitute.For<IAIDispatchService>();
    private readonly CancelAIDispatchTurnHandler sut;

    public CancelAIDispatchTurnHandlerTests()
    {
        sut = new CancelAIDispatchTurnHandler(ctx.TenantUow, dispatchService);
    }

    [Fact]
    public async Task Handle_ConversationNotFound_Fails()
    {
        var result = await sut.Handle(
            new CancelAIDispatchTurnCommand { ConversationId = Guid.NewGuid() }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await dispatchService.DidNotReceiveWithAnyArgs().CancelAsync(default, default);
    }

    [Fact]
    public async Task Handle_CopilotKindConversation_Fails()
    {
        var conversation = ctx.SetConversation(kind: AgentConversationKind.Copilot);

        var result = await sut.Handle(
            new CancelAIDispatchTurnCommand { ConversationId = conversation.Id }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await dispatchService.DidNotReceiveWithAnyArgs().CancelAsync(default, default);
    }

    /// <summary>Cancellation is cooperative - a live session is cancelled through the service, not by touching the conversation directly.</summary>
    [Fact]
    public async Task Handle_RunningSessionExists_DelegatesToDispatchServiceAndLeavesConversationAlone()
    {
        var conversation = ctx.SetConversation(kind: AgentConversationKind.Dispatch);
        conversation.BeginTurn();
        var session = new AgentSession
        {
            ConversationId = conversation.Id,
            Type = AgentSessionType.Dispatch
        };
        ctx.SessionRepo.Query().Returns(new List<AgentSession> { session }.BuildMock());

        var result = await sut.Handle(
            new CancelAIDispatchTurnCommand { ConversationId = conversation.Id }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await dispatchService.Received(1).CancelAsync(session.Id, Arg.Any<CancellationToken>());
        Assert.Equal(AgentConversationStatus.Running, conversation.Status);
        await ctx.TenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>No live session behind a Running conversation means a crashed turn - end it directly to un-stick the chat.</summary>
    [Fact]
    public async Task Handle_NoRunningSession_UnstucksTheConversationDirectly()
    {
        var conversation = ctx.SetConversation(kind: AgentConversationKind.Dispatch);
        conversation.BeginTurn();
        ctx.SessionRepo.Query().Returns(new List<AgentSession>().BuildMock());

        var result = await sut.Handle(
            new CancelAIDispatchTurnCommand { ConversationId = conversation.Id }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(AgentConversationStatus.Idle, conversation.Status);
        await dispatchService.DidNotReceiveWithAnyArgs().CancelAsync(default, default);
        await ctx.TenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>A session that already finished must not be mistaken for a live one to cancel.</summary>
    [Fact]
    public async Task Handle_OnlyCompletedSessionExists_TreatedAsNoRunningSession()
    {
        var conversation = ctx.SetConversation(kind: AgentConversationKind.Dispatch);
        conversation.BeginTurn();
        var session = new AgentSession { ConversationId = conversation.Id, Type = AgentSessionType.Dispatch };
        session.Complete();
        ctx.SessionRepo.Query().Returns(new List<AgentSession> { session }.BuildMock());

        var result = await sut.Handle(
            new CancelAIDispatchTurnCommand { ConversationId = conversation.Id }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(AgentConversationStatus.Idle, conversation.Status);
        await dispatchService.DidNotReceiveWithAnyArgs().CancelAsync(default, default);
    }
}
