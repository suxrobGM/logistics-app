using Logistics.Application.Tests.TestKit;
using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Modules.Integrations.AIDispatch.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.AIDispatch;

public class AgentTrustServiceTests
{
    private static readonly DateTime Start = new(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly List<AgentDecision> _decisions = [];
    private readonly AIDispatchPolicy _policy = new();
    private readonly List<AIDispatchPolicy> _policies = [];
    private readonly AgentSession _dispatchSession = new() { Type = AgentSessionType.Dispatch };
    private readonly AgentTrustService _sut;

    private static readonly AgentToolDefinition CreateTrip = new("create_trip", "")
    {
        DecisionType = AgentDecisionType.CreateTrip
    };

    public AgentTrustServiceTests()
    {
        _tenantUow.Stub<AgentDecision>(_decisions);

        _tenantUow.Stub<AIDispatchPolicy>(_policies);
        _policies.Add(_policy);

        _sut = new AgentTrustService(_tenantUow);
    }

    private void Add(AgentDecisionStatus status, int minute, DateTime? revertedAt = null,
        AgentDecisionType type = AgentDecisionType.CreateTrip, AgentSession? session = null)
    {
        var decision = new AgentDecision
        {
            Session = session ?? _dispatchSession,
            Type = type,
            Status = status,
            CreatedAt = Start.AddMinutes(minute)
        };

        if (revertedAt is { } at)
        {
            decision.MarkReverted(at);
        }

        _decisions.Add(decision);
    }

    #region Streak

    [Fact]
    public async Task GetStreak_CountsApprovalsSinceTheLastRejection()
    {
        Add(AgentDecisionStatus.Executed, 1);
        Add(AgentDecisionStatus.Rejected, 2);
        Add(AgentDecisionStatus.Executed, 3);
        Add(AgentDecisionStatus.Executed, 4);
        Add(AgentDecisionStatus.Suggested, 5);

        Assert.Equal(2, await _sut.GetStreakAsync(AgentDecisionType.CreateTrip));
    }

    [Fact]
    public async Task GetStreak_RevertResetsTheStreak()
    {
        Add(AgentDecisionStatus.Executed, 1);
        Add(AgentDecisionStatus.Executed, 2, revertedAt: Start.AddMinutes(3));
        Add(AgentDecisionStatus.Executed, 4);

        Assert.Equal(1, await _sut.GetStreakAsync(AgentDecisionType.CreateTrip));
    }

    [Fact]
    public async Task GetStreak_IgnoresOtherTypes()
    {
        Add(AgentDecisionStatus.Executed, 1);
        Add(AgentDecisionStatus.Rejected, 2, type: AgentDecisionType.AssignLoad);

        Assert.Equal(1, await _sut.GetStreakAsync(AgentDecisionType.CreateTrip));
    }

    #endregion

    #region Eligibility

    [Fact]
    public async Task CanAutoExecute_OptedInWithLongEnoughStreak_IsTrue()
    {
        _policy.SetAutoApprove([AgentDecisionType.CreateTrip], 2, 5, null);
        Add(AgentDecisionStatus.Executed, 1);
        Add(AgentDecisionStatus.Executed, 2);

        Assert.True(await _sut.CanAutoExecuteAsync(CreateTrip));
    }

    [Fact]
    public async Task CanAutoExecute_StreakTooShort_IsFalse()
    {
        _policy.SetAutoApprove([AgentDecisionType.CreateTrip], 3, 5, null);
        Add(AgentDecisionStatus.Executed, 1);

        Assert.False(await _sut.CanAutoExecuteAsync(CreateTrip));
    }

    [Fact]
    public async Task CanAutoExecute_NotOptedIn_IsFalse()
    {
        _policy.SetAutoApprove([], 1, 5, null);
        Add(AgentDecisionStatus.Executed, 1);

        Assert.False(await _sut.CanAutoExecuteAsync(CreateTrip));
    }

    [Fact]
    public async Task CanAutoExecute_IneligibleType_IsFalseEvenIfOptedIn()
    {
        _policy.SetAutoApprove([AgentDecisionType.BookLoadBoardLoad], 1, 5, null);
        var book = new AgentToolDefinition("book_load_board_load", "") { DecisionType = AgentDecisionType.BookLoadBoardLoad };
        Add(AgentDecisionStatus.Executed, 1, type: AgentDecisionType.BookLoadBoardLoad);

        Assert.False(await _sut.CanAutoExecuteAsync(book));
        Assert.Empty(_policy.AutoApproveTypes);
    }

    [Fact]
    public async Task CanAutoExecute_DestructiveTool_IsFalse()
    {
        _policy.SetAutoApprove([AgentDecisionType.CreateTrip], 1, 5, null);
        Add(AgentDecisionStatus.Executed, 1);

        Assert.False(await _sut.CanAutoExecuteAsync(CreateTrip with { Destructive = true }));
    }

    #endregion

    #region Revert window

    [Fact]
    public void CanRevert_AutoApprovedInsideTheWindow_IsTrueThenExpires()
    {
        var decision = new AgentDecision { Type = AgentDecisionType.CreateTrip };
        decision.MarkAutoExecuted();
        var executedAt = decision.ExecutedAt!.Value;

        Assert.True(decision.CanRevert(executedAt.AddMinutes(14)));
        Assert.False(decision.CanRevert(executedAt.Add(AgentDecision.RevertWindow).AddSeconds(1)));
    }

    [Fact]
    public void CanRevert_HumanApprovedOrAlreadyReverted_IsFalse()
    {
        var approved = new AgentDecision { Type = AgentDecisionType.CreateTrip };
        approved.Approve(Guid.NewGuid());
        approved.MarkExecuted();
        var reverted = new AgentDecision { Type = AgentDecisionType.CreateTrip };
        reverted.MarkAutoExecuted();
        reverted.MarkReverted(DateTime.UtcNow);

        Assert.False(approved.CanRevert(DateTime.UtcNow));
        Assert.False(reverted.CanRevert(DateTime.UtcNow));
    }

    #endregion
}
