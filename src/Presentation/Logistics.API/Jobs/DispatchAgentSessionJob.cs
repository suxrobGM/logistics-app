using Hangfire;
using Logistics.Application.Services;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.API.Jobs;

/// <summary>
///     Hangfire job that runs a single dispatch agent session for a specific tenant.
///     Enqueued by <see cref="HangfireDispatchRunner"/> when a user or re-plan triggers a session.
/// </summary>
public class DispatchAgentSessionJob(
    IServiceScopeFactory scopeFactory,
    ILogger<DispatchAgentSessionJob> logger)
{
    [AutomaticRetry(Attempts = 0)]
    public async Task RunAsync(
        Guid tenantId,
        DispatchAgentMode mode,
        Guid? triggeredByUserId,
        string? instructions,
        string? rejectionContext,
        bool isOverage,
        CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var tenantUow = scope.ServiceProvider.GetRequiredService<ITenantUnitOfWork>();
        await tenantUow.SetCurrentTenantByIdAsync(tenantId);

        var agentService = scope.ServiceProvider.GetRequiredService<IDispatchAgentService>();

        try
        {
            await agentService.RunAsync(new DispatchAgentRequest(
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
public class HangfireDispatchJobRunner(IBackgroundJobClient jobClient) : IBackgroundJobRunner<DispatchAgentRequest>
{
    public void Enqueue(DispatchAgentRequest request)
    {
        jobClient.Enqueue<DispatchAgentSessionJob>(job => job.RunAsync(
            request.TenantId,
            request.Mode,
            request.TriggeredByUserId,
            request.Instructions,
            request.RejectionContext,
            request.IsOverage,
            CancellationToken.None));
    }
}
