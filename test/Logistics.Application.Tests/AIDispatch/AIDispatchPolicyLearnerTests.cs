using Logistics.Application.Abstractions.AI;
using Logistics.Application.Modules.Integrations.AIDispatch.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.AIDispatch;

public class AIDispatchPolicyLearnerTests
{
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ILlmClient llmClient = Substitute.For<ILlmClient>();
    private readonly ITenantRepository<AgentDecision, Guid> decisionRepo =
        Substitute.For<ITenantRepository<AgentDecision, Guid>>();
    private readonly ITenantRepository<AIDispatchPolicy, Guid> policyRepo =
        Substitute.For<ITenantRepository<AIDispatchPolicy, Guid>>();

    private readonly AIDispatchPolicyLearner sut;

    public AIDispatchPolicyLearnerTests()
    {
        tenantUow.GetCurrentTenant().Returns(new Tenant
        {
            Name = "Test Fleet",
            ConnectionString = "test",
            BillingEmail = "billing@test.com",
            CompanyAddress = new Address
            {
                Line1 = "1 Test St", City = "Test", State = "TX", ZipCode = "00000", Country = "US"
            }
        });

        tenantUow.Repository<AgentDecision>().Returns(decisionRepo);
        tenantUow.Repository<AIDispatchPolicy>().Returns(policyRepo);

        SetDecisions();
        SetPolicy(null);

        var llmOptions = Options.Create(new LlmOptions { PolicyLearningModel = "deepseek-v4-flash" });

        sut = new AIDispatchPolicyLearner(
            tenantUow, llmClient, llmOptions, NullLogger<AIDispatchPolicyLearner>.Instance);
    }

    #region Minimum-evidence gate

    [Fact]
    public async Task Learn_TooFewDecisions_SkipsWithoutCallingLlm()
    {
        SetDecisions(Rejections(5));

        var result = await sut.LearnForCurrentTenantAsync();

        Assert.True(result.IsSuccess);
        Assert.False(result.Value!.Generated);
        Assert.Contains("Not enough reviewed decisions", result.Value.SkipReason);
        await llmClient.DidNotReceiveWithAnyArgs().CompleteAsync(default!, default);
    }

    /// <summary>
    /// A couple of rejections give the model nothing to generalise from, so the rejection floor is
    /// enforced separately from the total count.
    /// </summary>
    [Fact]
    public async Task Learn_EnoughDecisionsButTooFewRejections_SkipsWithoutCallingLlm()
    {
        SetDecisions([.. Rejections(2), .. Approvals(20)]);

        var result = await sut.LearnForCurrentTenantAsync();

        Assert.False(result.Value!.Generated);
        await llmClient.DidNotReceiveWithAnyArgs().CompleteAsync(default!, default);
    }

    /// <summary>The floors hold even when the dispatcher forces a regeneration.</summary>
    [Fact]
    public async Task Learn_ForcedButTooFewDecisions_StillSkips()
    {
        SetDecisions(Rejections(4));

        var result = await sut.LearnForCurrentTenantAsync(force: true);

        Assert.False(result.Value!.Generated);
        await llmClient.DidNotReceiveWithAnyArgs().CompleteAsync(default!, default);
    }

    #endregion

    #region Which decisions count

    /// <summary>
    /// Autonomous executions would train the agent on its own output; Query decisions are read tool
    /// calls carrying no verdict at all.
    /// </summary>
    [Fact]
    public async Task Learn_ExcludesAutonomousExecutionsAndQueries()
    {
        var autonomous = Enumerable.Range(0, 30)
            .Select(_ => Decision(AgentDecisionStatus.Executed, approvedBy: null, toolName: "auto_tool"))
            .ToList();
        var queries = Enumerable.Range(0, 30)
            .Select(_ => Decision(
                AgentDecisionStatus.Executed, approvedBy: Guid.NewGuid(),
                type: AgentDecisionType.Query, toolName: "query_tool"))
            .ToList();

        // Enough qualifying rows to clear the evidence gate, so only the filter can keep the noise out.
        SetDecisions([.. Rejections(5), .. Approvals(15), .. autonomous, .. queries]);
        SetLlmResponse("## Learned preferences\n- Prefer short hauls (5 rejections)");

        await sut.LearnForCurrentTenantAsync();

        var request = CapturedRequest();
        Assert.DoesNotContain("auto_tool", request.UserText);
        Assert.DoesNotContain("query_tool", request.UserText);
    }

    [Fact]
    public async Task Learn_IncludesHumanApprovedExecutions()
    {
        SetDecisions([.. Rejections(5), .. Approvals(15)]);
        SetLlmResponse("## Learned preferences\n- Prefer short hauls (5 rejections)");

        await sut.LearnForCurrentTenantAsync();

        var request = CapturedRequest();
        Assert.Contains("APPROVED", request.UserText);
        Assert.Contains("REJECTED", request.UserText);
    }

    #endregion

    #region Watermark

    [Fact]
    public async Task Learn_NoNewDecisionsSinceLastRun_Skips()
    {
        var decisions = Rejections(20);
        SetDecisions(decisions);

        var policy = new AIDispatchPolicy();
        policy.ApplyLearnedPolicy("old", 20, decisions.Max(d => d.CreatedAt), "deepseek-v4-flash", 0.001m);
        SetPolicy(policy);

        var result = await sut.LearnForCurrentTenantAsync();

        Assert.False(result.Value!.Generated);
        Assert.Contains("No new decisions", result.Value.SkipReason);
        await llmClient.DidNotReceiveWithAnyArgs().CompleteAsync(default!, default);
    }

    [Fact]
    public async Task Learn_ForcedWithNoNewDecisions_RunsAnyway()
    {
        var decisions = Rejections(20);
        SetDecisions(decisions);

        var policy = new AIDispatchPolicy();
        policy.ApplyLearnedPolicy("old", 20, decisions.Max(d => d.CreatedAt), "deepseek-v4-flash", 0.001m);
        // Push LastRunAt outside the manual cooldown.
        typeof(AIDispatchPolicy).GetProperty(nameof(AIDispatchPolicy.LastRunAt))!
            .SetValue(policy, DateTime.UtcNow.AddHours(-2));
        SetPolicy(policy);

        SetLlmResponse("## Learned preferences\n- Prefer short hauls (20 rejections)");

        var result = await sut.LearnForCurrentTenantAsync(force: true);

        Assert.True(result.Value!.Generated);
    }

    [Fact]
    public async Task Learn_ForcedWithinCooldown_Skips()
    {
        SetDecisions(Rejections(20));

        var policy = new AIDispatchPolicy();
        policy.MarkRunCompleted();
        SetPolicy(policy);

        var result = await sut.LearnForCurrentTenantAsync(force: true);

        Assert.False(result.Value!.Generated);
        Assert.Contains("less than 10 minutes ago", result.Value.SkipReason);
    }

    #endregion

    #region Output handling

    [Fact]
    public async Task Learn_HappyPath_StoresPolicyAndAdvancesWatermark()
    {
        var decisions = Rejections(20);
        SetDecisions(decisions);
        SetLlmResponse("## Learned preferences\n- Prefer short hauls (20 rejections)");

        AIDispatchPolicy? added = null;
        await policyRepo.AddAsync(Arg.Do<AIDispatchPolicy>(p => added = p), Arg.Any<CancellationToken>());

        var result = await sut.LearnForCurrentTenantAsync();

        Assert.True(result.Value!.Generated);
        Assert.Equal(20, result.Value.DecisionsAnalyzed);
        Assert.NotNull(added);
        Assert.Contains("Prefer short hauls", added!.GeneratedContent);
        Assert.Equal(decisions.Max(d => d.CreatedAt), added.LastDecisionAt);
        Assert.Equal("deepseek-v4-flash", added.ModelUsed);
        await tenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>A policy the evidence no longer supports must stop steering the agent.</summary>
    [Fact]
    public async Task Learn_NoPolicyToken_ClearsContentButAdvancesWatermark()
    {
        var decisions = Rejections(20);
        SetDecisions(decisions);
        SetLlmResponse("NO_POLICY");

        var policy = new AIDispatchPolicy();
        policy.ApplyLearnedPolicy("## Learned preferences\n- Stale rule (3 rejections)", 5, null, "m", 0m);
        SetPolicy(policy);

        var result = await sut.LearnForCurrentTenantAsync();

        Assert.False(result.Value!.Generated);
        Assert.Null(policy.GeneratedContent);
        Assert.Equal(decisions.Max(d => d.CreatedAt), policy.LastDecisionAt);
    }

    [Fact]
    public async Task Learn_NeverTouchesManualContent()
    {
        SetDecisions(Rejections(20));
        SetLlmResponse("## Learned preferences\n- Prefer short hauls (20 rejections)");

        var policy = new AIDispatchPolicy();
        policy.EditManual("- My own rule", isEnabled: true, Guid.NewGuid());
        SetPolicy(policy);

        await sut.LearnForCurrentTenantAsync();

        Assert.Equal("- My own rule", policy.ManualContent);
        Assert.Contains("Prefer short hauls", policy.GeneratedContent);
    }

    /// <summary>
    /// Storage clamping is the entity's invariant (the wider column budget); the injection cap belongs
    /// to the prompt builder. Either way a stored bullet is whole.
    /// </summary>
    [Fact]
    public async Task Learn_OverlongOutput_ClampsToStorageBudgetAtLineBoundary()
    {
        SetDecisions(Rejections(20));
        var bullet = "- " + new string('z', 100);
        SetLlmResponse(string.Join('\n', Enumerable.Repeat(bullet, 200)));

        AIDispatchPolicy? added = null;
        await policyRepo.AddAsync(Arg.Do<AIDispatchPolicy>(p => added = p), Arg.Any<CancellationToken>());

        await sut.LearnForCurrentTenantAsync();

        Assert.NotNull(added?.GeneratedContent);
        Assert.True(
            added!.GeneratedContent!.Length <= DispatchPolicyText.MaxStoredChars,
            $"stored {added.GeneratedContent.Length} chars");
        Assert.All(
            added.GeneratedContent.Split('\n'),
            line => Assert.Equal(bullet, line));
    }

    #endregion

    #region Gates and failures

    [Fact]
    public async Task Learn_PolicyDisabled_Skips()
    {
        SetDecisions(Rejections(20));

        var policy = new AIDispatchPolicy();
        policy.EditManual(null, isEnabled: false, Guid.NewGuid());
        SetPolicy(policy);

        var result = await sut.LearnForCurrentTenantAsync();

        Assert.False(result.Value!.Generated);
        Assert.Contains("switched off", result.Value.SkipReason);
        await llmClient.DidNotReceiveWithAnyArgs().CompleteAsync(default!, default);
    }

    /// <summary>A failed pass leaves the watermark alone so tomorrow's run retries the same history.</summary>
    [Fact]
    public async Task Learn_LlmFails_ReturnsFailAndLeavesWatermarkUntouched()
    {
        SetDecisions(Rejections(20));

        var policy = new AIDispatchPolicy();
        SetPolicy(policy);

        llmClient.CompleteAsync(Arg.Any<LlmCompletionRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<LlmCompletionResult>.Fail("provider exploded"));

        var result = await sut.LearnForCurrentTenantAsync();

        Assert.False(result.IsSuccess);
        Assert.Null(policy.LastDecisionAt);
        Assert.Null(policy.GeneratedContent);
    }

    [Fact]
    public async Task Learn_PassesConfiguredLearningModel()
    {
        SetDecisions(Rejections(20));
        SetLlmResponse("## Learned preferences\n- Prefer short hauls (20 rejections)");

        await sut.LearnForCurrentTenantAsync();

        Assert.Equal("deepseek-v4-flash", CapturedRequest().ModelId);
    }

    /// <summary>The existing policy is fed back so wording stays stable instead of flapping nightly.</summary>
    [Fact]
    public async Task Learn_SeedsPromptWithExistingPolicy()
    {
        SetDecisions(Rejections(20));
        SetLlmResponse("## Learned preferences\n- Prefer short hauls (20 rejections)");

        var policy = new AIDispatchPolicy();
        policy.ApplyLearnedPolicy("## Learned preferences\n- Existing rule (4 rejections)", 4, null, "m", 0m);
        SetPolicy(policy);

        await sut.LearnForCurrentTenantAsync();

        Assert.Contains("Existing rule (4 rejections)", CapturedRequest().UserText);
    }

    #endregion

    #region Helpers

    private void SetDecisions(List<AgentDecision>? decisions = null)
    {
        decisionRepo.Query().Returns((decisions ?? []).BuildMock());
    }

    private void SetPolicy(AIDispatchPolicy? policy)
    {
        var list = policy is null ? new List<AIDispatchPolicy>() : [policy];
        policyRepo.Query().Returns(list.BuildMock());
    }

    private void SetLlmResponse(string text)
    {
        llmClient.CompleteAsync(Arg.Any<LlmCompletionRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<LlmCompletionResult>.Ok(
                new LlmCompletionResult(text, "deepseek-v4-flash", 1200, 300, 0.0004m)));
    }

    private LlmCompletionRequest CapturedRequest()
    {
        return (LlmCompletionRequest)llmClient.ReceivedCalls()
            .Single(c => c.GetMethodInfo().Name == nameof(ILlmClient.CompleteAsync))
            .GetArguments()[0]!;
    }

    private static List<AgentDecision> Rejections(int count)
    {
        return Enumerable.Range(0, count)
            .Select(i => Decision(
                AgentDecisionStatus.Rejected,
                approvedBy: Guid.NewGuid(),
                rejectionReason: "Deadhead over 80 miles",
                minutesAgo: i))
            .ToList();
    }

    private static List<AgentDecision> Approvals(int count)
    {
        return Enumerable.Range(0, count)
            .Select(i => Decision(
                AgentDecisionStatus.Executed,
                approvedBy: Guid.NewGuid(),
                minutesAgo: 100 + i))
            .ToList();
    }

    private static AgentDecision Decision(
        AgentDecisionStatus status,
        Guid? approvedBy,
        AgentDecisionType type = AgentDecisionType.AssignLoad,
        string toolName = "assign_load_to_truck",
        string? rejectionReason = null,
        int minutesAgo = 0)
    {
        return new AgentDecision
        {
            Status = status,
            Type = type,
            ToolName = toolName,
            ToolInput = """{"load_id":"abc","truck_id":"def"}""",
            Reasoning = "Closest available truck",
            RejectionReason = rejectionReason,
            ApprovedByUserId = approvedBy,
            CreatedAt = DateTime.UtcNow.AddMinutes(-minutesAgo),
            // The learner filters on Session.Type - a null nav would NRE the mocked queryable.
            Session = new AgentSession { Mode = AgentAutonomyMode.HumanInTheLoop }
        };
    }

    #endregion
}
