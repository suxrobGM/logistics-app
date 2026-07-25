using Hangfire;
using Logistics.Application.Abstractions.Features;
using Logistics.Application.Modules.Integrations.AIDispatch.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.API.Jobs;

/// <summary>
///     Nightly job that turns each tenant's AI dispatch approve/reject history into a short
///     dispatch policy injected into the agent's system prompt.
///     <para>
///     Daily rather than weekly: the learner's watermark makes a quiet night cost one query and no
///     tokens, so a new preference can reach the agent within 24h of the rejection that taught it.
///     </para>
/// </summary>
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

    /// <summary>
    ///     Retries are safe: tenants the failed attempt already processed have an advanced watermark
    ///     and skip without calling the LLM again.
    /// </summary>
    [AutomaticRetry(Attempts = 2)]
    public Task ProcessAllTenantsAsync(CancellationToken ct) =>
        TenantJobRunner.ForEachTenantAsync(
            scopeFactory, logger, "AI dispatch policy learning", ProcessTenantAsync, ct);

    private async Task ProcessTenantAsync(IServiceScope scope, Tenant tenant, CancellationToken ct)
    {
        var featureService = scope.ServiceProvider.GetRequiredService<IFeatureService>();

        // [RequiresFeature] is a MediatR pipeline behavior and inert here, so the job checks itself.
        if (!await featureService.IsFeatureEnabledAsync(tenant.Id, TenantFeature.AgenticDispatch))
        {
            return;
        }

        var tenantUow = scope.ServiceProvider.GetRequiredService<ITenantUnitOfWork>();
        tenantUow.SetCurrentTenant(tenant);

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
