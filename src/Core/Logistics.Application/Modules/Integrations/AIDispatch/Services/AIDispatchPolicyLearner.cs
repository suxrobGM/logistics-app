using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.AI;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Services;

internal sealed class AIDispatchPolicyLearner(
    ITenantUnitOfWork tenantUow,
    ILlmClient llmClient,
    IOptions<LlmOptions> llmOptions,
    ILogger<AIDispatchPolicyLearner> logger) : IAIDispatchPolicyLearner
{
    /// <summary>How far back to look. Older preferences are likely stale.</summary>
    private const int LookbackDays = 60;

    /// <summary>Below this the model has nothing to generalise from and starts inventing rules.</summary>
    private const int MinDecisions = 15;
    private const int MinRejections = 3;

    /// <summary>Rate-limits manual regeneration. The nightly pass uses the watermark instead.</summary>
    private static readonly TimeSpan ManualRerunCooldown = TimeSpan.FromMinutes(10);

    public async Task<Result<AIDispatchPolicyLearningOutcome>> LearnForCurrentTenantAsync(
        bool force = false,
        CancellationToken ct = default)
    {
        var tenant = tenantUow.GetCurrentTenant();
        var bypassGate = llmOptions.Value.BypassAIGate;

        if (!bypassGate && tenant.Settings.AIEnabled == false)
        {
            return Skipped("AI is disabled for this tenant");
        }

        var policyRepo = tenantUow.Repository<AIDispatchPolicy>();
        var policy = await policyRepo.Query().FirstOrDefaultAsync(ct);

        if (policy is not null && !policy.IsEnabled)
        {
            return Skipped("Policy learning is switched off");
        }

        if (force && policy?.LastRunAt is not null &&
            DateTime.UtcNow - policy.LastRunAt.Value < ManualRerunCooldown)
        {
            return Skipped("Policy was regenerated less than 10 minutes ago");
        }

        // Cheapest gate first: on a quiet night this aggregate is the only query the tenant costs.
        if (!force && policy?.LastDecisionAt is not null)
        {
            var newestDecisionAt = await QualifyingHistory().MaxAsync(d => (DateTime?)d.CreatedAt, ct);
            if (newestDecisionAt <= policy.LastDecisionAt)
            {
                return Skipped("No new decisions since the last run");
            }
        }

        var entries = await LoadHistoryAsync(ct);
        var digest = DecisionHistoryDigest.Build(entries);

        if (digest.Count < MinDecisions || digest.RejectionCount < MinRejections)
        {
            // Only manual attempts are cooldown-guarded; stamping the nightly pass would be a
            // pointless UPDATE for every tenant short on history.
            if (force && policy is not null)
            {
                policy.MarkRunCompleted();
                await tenantUow.SaveChangesAsync(ct);
            }

            return Skipped(
                $"Not enough reviewed decisions yet - need {MinDecisions} ({MinRejections} rejections), have {digest.Count} ({digest.RejectionCount})");
        }

        var completion = await llmClient.CompleteAsync(
            new LlmCompletionRequest
            {
                SystemPrompt = AIDispatchPolicyPrompt.SystemPrompt,
                UserText = AIDispatchPolicyPrompt.BuildUserText(digest.Text, policy?.GeneratedContent),
                MaxTokens = 1024,
                ModelId = llmOptions.Value.PolicyLearningModel
            },
            ct);

        if (!completion.IsSuccess || completion.Value is null)
        {
            // Leave the watermark untouched so tonight's failure retries tomorrow.
            logger.LogWarning(
                "Policy learning failed for tenant {TenantId}: {Error}", tenant.Id, completion.Error);
            return Result<AIDispatchPolicyLearningOutcome>.Fail(
                completion.Error ?? "Policy learning failed.");
        }

        var result = completion.Value;
        var content = NormalizeOutput(result.Text);

        policy ??= await CreatePolicyAsync(policyRepo, ct);
        policy.ApplyLearnedPolicy(
            content, digest.Count, digest.LastDecisionAt, result.ModelUsed, result.EstimatedCostUsd);

        await tenantUow.SaveChangesAsync(ct);

        logger.LogInformation(
            "Learned dispatch policy for tenant {TenantId} from {DecisionCount} decisions ({RejectionCount} rejections), model {Model}, est ${Cost:F4}{Empty}",
            tenant.Id, digest.Count, digest.RejectionCount, result.ModelUsed, result.EstimatedCostUsd,
            content is null ? " - no rule met the evidence bar" : "");

        return Result<AIDispatchPolicyLearningOutcome>.Ok(
            new AIDispatchPolicyLearningOutcome(
                content is not null, null, digest.Count, result.EstimatedCostUsd));
    }

    /// <summary>
    /// The labelled history: rejections, plus executions a human approved. Autonomous executions carry
    /// no human signal (null <c>ApprovedByUserId</c>) - including them would train the agent on itself.
    /// </summary>
    private IQueryable<AgentDecision> QualifyingHistory()
    {
        var cutoff = DateTime.UtcNow.AddDays(-LookbackDays);

        return tenantUow.Repository<AgentDecision>().Query()
            .DispatchOnly()
            .Where(d => d.Type != AgentDecisionType.Query)
            .Where(d => d.CreatedAt >= cutoff)
            .Where(d => d.Status == AgentDecisionStatus.Rejected ||
                        (d.Status == AgentDecisionStatus.Executed && d.ApprovedByUserId != null));
    }

    private async Task<List<DecisionHistoryEntry>> LoadHistoryAsync(CancellationToken ct)
    {
        // Newest first, then capped - taking from the newest end keeps the watermark correct
        // without reading the whole window.
        return await QualifyingHistory()
            .OrderByDescending(d => d.CreatedAt)
            .Take(DecisionHistoryDigest.MaxDecisions)
            .Select(d => new DecisionHistoryEntry(
                d.Status, d.ToolName, d.ToolInput, d.Reasoning, d.RejectionReason, d.CreatedAt))
            .ToListAsync(ct);
    }

    private static async Task<AIDispatchPolicy> CreatePolicyAsync(
        ITenantRepository<AIDispatchPolicy, Guid> repo,
        CancellationToken ct)
    {
        var policy = new AIDispatchPolicy();
        await repo.AddAsync(policy, ct);
        return policy;
    }

    /// <summary>
    /// The no-policy token and empty output both mean "clear the learned section". Length clamping is
    /// not repeated here - it belongs to <see cref="AIDispatchPolicy.ApplyLearnedPolicy"/> (storage)
    /// and the prompt builder (injection).
    /// </summary>
    private static string? NormalizeOutput(string text)
    {
        var trimmed = text.Trim();

        return trimmed.Length == 0 ||
               trimmed.Equals(AIDispatchPolicyPrompt.NoPolicyToken, StringComparison.OrdinalIgnoreCase)
            ? null
            : trimmed;
    }

    private static Result<AIDispatchPolicyLearningOutcome> Skipped(string reason) =>
        Result<AIDispatchPolicyLearningOutcome>.Ok(
            new AIDispatchPolicyLearningOutcome(false, reason, 0, 0m));
}
