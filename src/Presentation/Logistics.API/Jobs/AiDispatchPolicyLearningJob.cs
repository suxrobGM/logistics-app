using Hangfire;
using Logistics.Application.Abstractions.Features;
using Logistics.Application.Modules.Integrations.AiDispatch.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.API.Jobs;

/// <summary>
///     Nightly job that turns each tenant's AI dispatch approve/reject history into a short
///     dispatch policy injected into the agent's system prompt.
///     <para>
///     Runs daily rather than weekly even though the policy changes slowly: the learner keeps a
///     watermark, so a night with no new decisions costs one query and no tokens. Daily means a new
///     preference reaches the agent within 24h of the rejection that taught it.
///     </para>
/// </summary>
public class AiDispatchPolicyLearningJob(
    ILogger<AiDispatchPolicyLearningJob> logger,
    IServiceScopeFactory scopeFactory)
{
    public static void ScheduleJobs()
    {
        RecurringJob.AddOrUpdate<AiDispatchPolicyLearningJob>(
            "ai-dispatch-policy-learning",
            job => job.ProcessAllTenantsAsync(CancellationToken.None),
            Cron.Daily(4));
    }

    /// <summary>
    ///     Retries are safe: tenants processed by the failed attempt have an advanced watermark and
    ///     skip without calling the LLM again.
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

        var learner = scope.ServiceProvider.GetRequiredService<IAiDispatchPolicyLearner>();
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
            // Debug: most tenants skip most nights, and at Information this drowns the job log.
            logger.LogDebug("Skipped policy learning for tenant {TenantName}: {Reason}",
                tenant.Name, outcome.SkipReason);
        }
    }
}
