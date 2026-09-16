using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Modules.Integrations.AICopilot.Commands;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.AICopilot;

public class CancelAICopilotTurnHandlerTests
{
    private readonly AgentTestContext _ctx = new();
    private readonly CancelAICopilotTurnHandler _sut;

    public CancelAICopilotTurnHandlerTests()
    {
        _sut = new CancelAICopilotTurnHandler(_ctx.Commands, _ctx.CurrentUser);
    }

    [Fact]
    public async Task Handle_ConversationNotFound_Fails()
    {
        var result = await _sut.Handle(
            new CancelAICopilotTurnCommand { ConversationId = Guid.NewGuid() }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await _ctx.DispatchService.DidNotReceiveWithAnyArgs().CancelAsync(default, default);
    }

    [Fact]
    public async Task Handle_NotOwner_Fails()
    {
        var conversation = _ctx.SetConversation(createdById: Guid.NewGuid(), kind: AgentConversationKind.Copilot);

        var result = await _sut.Handle(
            new CancelAICopilotTurnCommand { ConversationId = conversation.Id }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await _ctx.DispatchService.DidNotReceiveWithAnyArgs().CancelAsync(default, default);
    }

    [Fact]
    public async Task Handle_DispatchKindConversation_Fails()
    {
        var conversation = _ctx.SetConversation(kind: AgentConversationKind.Dispatch);

        var result = await _sut.Handle(
            new CancelAICopilotTurnCommand { ConversationId = conversation.Id }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await _ctx.DispatchService.DidNotReceiveWithAnyArgs().CancelAsync(default, default);
    }

    /// <summary>Cancellation is cooperative - a live session is cancelled through the service, not by touching the conversation directly.</summary>
    [Fact]
    public async Task Handle_RunningSessionExists_DelegatesToDispatchServiceAndLeavesConversationAlone()
    {
        var conversation = _ctx.SetConversation(kind: AgentConversationKind.Copilot);
        conversation.BeginTurn();
        var session = new AgentSession { ConversationId = conversation.Id, Type = AgentSessionType.Copilot };
        _ctx.SessionRepo.Query().Returns(new List<AgentSession> { session }.BuildMock());

        var result = await _sut.Handle(
            new CancelAICopilotTurnCommand { ConversationId = conversation.Id }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _ctx.DispatchService.Received(1).CancelAsync(session.Id, Arg.Any<CancellationToken>());
        Assert.Equal(AgentConversationStatus.Running, conversation.Status);
        await _ctx.TenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>No live session behind a Running conversation means a crashed turn - end it directly to un-stick the chat.</summary>
    [Fact]
    public async Task Handle_NoRunningSession_UnstucksTheConversationDirectly()
    {
        var conversation = _ctx.SetConversation(kind: AgentConversationKind.Copilot);
        conversation.BeginTurn();
        _ctx.SessionRepo.Query().Returns(new List<AgentSession>().BuildMock());

        var result = await _sut.Handle(
            new CancelAICopilotTurnCommand { ConversationId = conversation.Id }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(AgentConversationStatus.Idle, conversation.Status);
        await _ctx.DispatchService.DidNotReceiveWithAnyArgs().CancelAsync(default, default);
        await _ctx.TenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
