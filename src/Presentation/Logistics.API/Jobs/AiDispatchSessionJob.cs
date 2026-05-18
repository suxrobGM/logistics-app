using Hangfire;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Application.Abstractions.BackgroundJobs;
using Logistics.Application.Abstractions.AiDispatch;

namespace Logistics.API.Jobs;

/// <summary>
///     Hangfire job that runs a single dispatch agent session for a specific tenant.
///     Enqueued by <see cref="HangfireAiDispatchRunner"/> when a user or re-plan triggers a session.
/// </summary>
public class AiDispatchSessionJob(
    IServiceScopeFactory scopeFactory,
    ILogger<AiDispatchSessionJob> logger)
{
    [AutomaticRetry(Attempts = 0)]
    public async Task RunAsync(
        Guid tenantId,
        AiDispatchMode mode,
        Guid? triggeredByUserId,
        string? instructions,
        string? rejectionContext,
        bool isOverage,
        CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var tenantUow = scope.ServiceProvider.GetRequiredService<ITenantUnitOfWork>();
        await tenantUow.SetCurrentTenantByIdAsync(tenantId);

        var agentService = scope.ServiceProvider.GetRequiredService<IAiDispatchService>();

        try
        {
            await agentService.RunAsync(new AiDispatchRequest(
                tenantId, mode, triggeredByUserId,
                IsOverage: isOverage,
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
public class HangfireAiDispatchJobRunner(IBackgroundJobClient jobClient) : IBackgroundJobRunner<AiDispatchRequest>
{
    public void Enqueue(AiDispatchRequest request)
    {
        jobClient.Enqueue<AiDispatchSessionJob>(job => job.RunAsync(
            request.TenantId,
            request.Mode,
            request.TriggeredByUserId,
            request.Instructions,
            request.RejectionContext,
            request.IsOverage,
            CancellationToken.None));
    }
}
