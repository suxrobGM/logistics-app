using Hangfire;
using Logistics.API.Jobs.Scheduling;
using Logistics.Application.Abstractions.Context;
using Logistics.Application.Modules.AI.AIDispatch.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.API.Jobs;

/// <summary>Daily job that updates each tenant's learned dispatch policy. A tenant with no new decisions since the learner's watermark costs one query and no LLM tokens.</summary>
public class AIDispatchPolicyLearningJob(
    ILogger<AIDispatchPolicyLearningJob> logger,
    IServiceScopeFactory scopeFactory)
{
    public static void ScheduleJobs()
    {
        RecurringJob.AddOrUpdate<AIDispatchPolicyLearningJob>(
            "ai-dispatch-policy-learning",
            job => job.ProcessAllTenantsAsync(CancellationToken.None),
            Cron.Daily(4));
    }

    /// <summary>Runs the learner for every tenant. Retrying is safe: tenants already processed have an advanced watermark and skip the LLM call.</summary>
    [AutomaticRetry(Attempts = 2)]
    public Task ProcessAllTenantsAsync(CancellationToken ct) =>
        TenantJobRunner.ForEachTenantAsync(
            scopeFactory, logger, "AI dispatch policy learning", ProcessTenantAsync, ct);

    private async Task ProcessTenantAsync(IServiceScope scope, Tenant tenant, CancellationToken ct)
    {
        var featureService = scope.ServiceProvider.GetRequiredService<IFeatureService>();

        // [RequiresFeature] is a pipeline behaviour and inert here, so the job checks itself.
        if (!await featureService.IsFeatureEnabledAsync(tenant.Id, TenantFeature.AIDispatch))
        {
            return;
        }

        var learner = scope.ServiceProvider.GetRequiredService<IAIDispatchPolicyLearner>();
        var result = await learner.LearnForCurrentTenantAsync(force: false, ct);

        if (!result.IsSuccess)
        {
            logger.LogWarning("Policy learning failed for tenant {TenantName}: {Error}",
                tenant.Name, result.Error);
            return;
        }

        var outcome = result.Value!;
        if (outcome.Generated)
        {
            logger.LogInformation(
                "Learned dispatch policy for tenant {TenantName} from {DecisionCount} decisions (est ${Cost:F4})",
                tenant.Name, outcome.DecisionsAnalyzed, outcome.CostUsd);
        }
        else
        {
            // Debug, not Information - most tenants skip most nights and would drown the job log.
            logger.LogDebug("Skipped policy learning for tenant {TenantName}: {Reason}",
                tenant.Name, outcome.SkipReason);
        }
    }
}
