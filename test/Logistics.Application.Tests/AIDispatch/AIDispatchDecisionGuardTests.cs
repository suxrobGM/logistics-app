using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.AIDispatch;

public class AIDispatchDecisionGuardTests
{
    private readonly AgentTestContext _ctx = new();

    [Fact]
    public async Task LoadAsync_AIDisabledForTenant_Fails()
    {
        _ctx.Tenant.Settings.AIEnabled = false;

        var result = await _ctx.DispatchGuard(bypassAIGate: false)
            .LoadAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("disabled", result.Error);
    }

    [Fact]
    public async Task LoadAsync_DecisionNotFound_Fails()
    {
        _ctx.DecisionRepo.Query().Returns(new List<AgentDecision>().BuildMock());

        var result = await _ctx.DispatchGuard().LoadAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Decision not found", result.Error);
    }

    /// <summary>A copilot decision id must never resolve through the dispatch guard.</summary>
    [Fact]
    public async Task LoadAsync_CopilotDecisionId_Fails()
    {
        var copilotSession = new AgentSession { Type = AgentSessionType.Copilot };
        var copilotDecision = new AgentDecision
        {
            SessionId = copilotSession.Id,
            Session = copilotSession,
            Status = AgentDecisionStatus.Suggested
        };
        _ctx.DecisionRepo.Query().Returns(new List<AgentDecision> { copilotDecision }.BuildMock());

        var result = await _ctx.DispatchGuard().LoadAsync(copilotDecision.Id, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Decision not found", result.Error);
    }

    [Fact]
    public async Task LoadAsync_DecisionNotSuggested_Fails()
    {
        var decision = _ctx.SetDispatchSuggestedDecision();
        decision.Approve(_ctx.UserId);

        var result = await _ctx.DispatchGuard().LoadAsync(decision.Id, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("suggested state", result.Error);
    }

    [Fact]
    public async Task LoadAsync_Success_ReturnsTheDecision()
    {
        var decision = _ctx.SetDispatchSuggestedDecision();

        var result = await _ctx.DispatchGuard().LoadAsync(decision.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(decision.Id, result.Value!.Id);
    }
}
