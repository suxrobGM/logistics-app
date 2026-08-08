using Hangfire;
using Logistics.Domain.Persistence;
using Logistics.Application.Abstractions.BackgroundJobs;
using Logistics.Application.Abstractions.AIDispatch;

namespace Logistics.API.Jobs;

/// <summary>
///     Hangfire job that runs a single dispatch agent session for a specific tenant.
///     Enqueued by <see cref="HangfireAIDispatchRunner"/> when a user or re-plan triggers a session.
/// </summary>
public class AIDispatchSessionJob(
    IServiceScopeFactory scopeFactory,
    ILogger<AIDispatchSessionJob> logger)
{
    [AutomaticRetry(Attempts = 0)]
    public async Task RunAsync(
        Guid tenantId,
        Guid? triggeredByUserId,
        string? instructions,
        string? rejectionContext,
        CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var tenantUow = scope.ServiceProvider.GetRequiredService<ITenantUnitOfWork>();
        await tenantUow.SetCurrentTenantByIdAsync(tenantId);

        var agentService = scope.ServiceProvider.GetRequiredService<IAIDispatchService>();

        try
        {
            await agentService.RunAsync(new AIDispatchRequest(
                tenantId, triggeredByUserId,
                Instructions: instructions,
                RejectionContext: rejectionContext), ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Dispatch agent session failed for tenant {TenantId}", tenantId);
        }
    }
}

/// <summary>
///     Hangfire-backed implementation of <see cref="IBackgroundJobRunner{T}"/> for dispatch agent sessions.
/// </summary>
public class HangfireAIDispatchJobRunner(IBackgroundJobClient jobClient) : IBackgroundJobRunner<AIDispatchRequest>
{
    public void Enqueue(AIDispatchRequest request)
    {
        jobClient.Enqueue<AIDispatchSessionJob>(job => job.RunAsync(
            request.TenantId,
            request.TriggeredByUserId,
            request.Instructions,
            request.RejectionContext,
            CancellationToken.None));
    }
}
