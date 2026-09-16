using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.AICopilot;

public class AICopilotDecisionGuardTests
{
    private readonly AgentTestContext _ctx = new();

    [Fact]
    public async Task LoadAsync_UserNotAuthenticated_Fails()
    {
        var result = await _ctx.CopilotGuard.LoadAsync(
            Guid.NewGuid(), userId: null, CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task LoadAsync_DecisionNotFound_Fails()
    {
        var result = await _ctx.CopilotGuard.LoadAsync(
            Guid.NewGuid(), _ctx.UserId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Decision not found", result.Error);
    }

    /// <summary>A dispatch decision id must never resolve through the copilot guard.</summary>
    [Fact]
    public async Task LoadAsync_DispatchDecisionId_Fails()
    {
        var dispatchSession = new AgentSession { Type = AgentSessionType.Dispatch };
        var dispatchDecision = new AgentDecision
        {
            SessionId = dispatchSession.Id,
            Session = dispatchSession,
            Status = AgentDecisionStatus.Suggested
        };
        _ctx.DecisionRepo.GetByIdAsync(dispatchDecision.Id, Arg.Any<CancellationToken>()).Returns(dispatchDecision);

        var result = await _ctx.CopilotGuard.LoadAsync(
            dispatchDecision.Id, _ctx.UserId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Decision not found", result.Error);
    }

    [Fact]
    public async Task LoadAsync_ConversationOwnedByAnotherUser_Fails()
    {
        var (decision, _) = _ctx.SetCopilotSuggestedDecision();
        _ctx.ConversationRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new AgentConversation { CreatedById = Guid.NewGuid(), Kind = AgentConversationKind.Copilot });

        var result = await _ctx.CopilotGuard.LoadAsync(
            decision.Id, _ctx.UserId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Decision not found", result.Error);
    }

    [Fact]
    public async Task LoadAsync_DecisionNotSuggested_Fails()
    {
        var (decision, _) = _ctx.SetCopilotSuggestedDecision();
        decision.Approve(_ctx.UserId);

        var result = await _ctx.CopilotGuard.LoadAsync(
            decision.Id, _ctx.UserId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("suggested state", result.Error);
    }

    [Fact]
    public async Task LoadAsync_Success_ReturnsDecisionAndConversation()
    {
        var (decision, conversation) = _ctx.SetCopilotSuggestedDecision();

        var result = await _ctx.CopilotGuard.LoadAsync(
            decision.Id, _ctx.UserId, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(decision.Id, result.Value!.Decision.Id);
        Assert.Equal(conversation.Id, result.Value.Conversation.Id);
    }
}
