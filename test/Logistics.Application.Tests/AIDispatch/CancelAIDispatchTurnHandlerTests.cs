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
    private readonly AgentTestContext _ctx = new();
    private readonly CancelAIDispatchTurnHandler _sut;

    public CancelAIDispatchTurnHandlerTests()
    {
        _sut = new CancelAIDispatchTurnHandler(_ctx.Commands);
    }

    [Fact]
    public async Task Handle_ConversationNotFound_Fails()
    {
        var result = await _sut.Handle(
            new CancelAIDispatchTurnCommand { ConversationId = Guid.NewGuid() }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await _ctx.DispatchService.DidNotReceiveWithAnyArgs().CancelAsync(default, default);
    }

    [Fact]
    public async Task Handle_CopilotKindConversation_Fails()
    {
        var conversation = _ctx.SetConversation(kind: AgentConversationKind.Copilot);

        var result = await _sut.Handle(
            new CancelAIDispatchTurnCommand { ConversationId = conversation.Id }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await _ctx.DispatchService.DidNotReceiveWithAnyArgs().CancelAsync(default, default);
    }

    /// <summary>Cancellation is cooperative - a live session is cancelled through the service, not by touching the conversation directly.</summary>
    [Fact]
    public async Task Handle_RunningSessionExists_DelegatesToDispatchServiceAndLeavesConversationAlone()
    {
        var conversation = _ctx.SetConversation(kind: AgentConversationKind.Dispatch);
        conversation.BeginTurn();
        var session = new AgentSession
        {
            ConversationId = conversation.Id,
            Type = AgentSessionType.Dispatch
        };
        _ctx.SessionRepo.Query().Returns(new List<AgentSession> { session }.BuildMock());

        var result = await _sut.Handle(
            new CancelAIDispatchTurnCommand { ConversationId = conversation.Id }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _ctx.DispatchService.Received(1).CancelAsync(session.Id, Arg.Any<CancellationToken>());
        Assert.Equal(AgentConversationStatus.Running, conversation.Status);
        await _ctx.TenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>No live session behind a Running conversation means a crashed turn - end it directly to un-stick the chat.</summary>
    [Fact]
    public async Task Handle_NoRunningSession_UnstucksTheConversationDirectly()
    {
        var conversation = _ctx.SetConversation(kind: AgentConversationKind.Dispatch);
        conversation.BeginTurn();
        _ctx.SessionRepo.Query().Returns(new List<AgentSession>().BuildMock());

        var result = await _sut.Handle(
            new CancelAIDispatchTurnCommand { ConversationId = conversation.Id }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(AgentConversationStatus.Idle, conversation.Status);
        await _ctx.DispatchService.DidNotReceiveWithAnyArgs().CancelAsync(default, default);
        await _ctx.TenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>A session that already finished must not be mistaken for a live one to cancel.</summary>
    [Fact]
    public async Task Handle_OnlyCompletedSessionExists_TreatedAsNoRunningSession()
    {
        var conversation = _ctx.SetConversation(kind: AgentConversationKind.Dispatch);
        conversation.BeginTurn();
        var session = new AgentSession { ConversationId = conversation.Id, Type = AgentSessionType.Dispatch };
        session.Complete();
        _ctx.SessionRepo.Query().Returns(new List<AgentSession> { session }.BuildMock());

        var result = await _sut.Handle(
            new CancelAIDispatchTurnCommand { ConversationId = conversation.Id }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(AgentConversationStatus.Idle, conversation.Status);
        await _ctx.DispatchService.DidNotReceiveWithAnyArgs().CancelAsync(default, default);
    }
}
