using Hangfire;
using Logistics.Application.Abstractions.BackgroundJobs;
using Logistics.Application.Abstractions.Features;
using Logistics.Application.Abstractions.Negotiation;
using Logistics.Application.Modules.Integrations.Negotiation.Services;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.API.Jobs;

/// <summary>
/// Retries waking the dispatch agent for a negotiation whose conversation was mid-turn when the
/// broker replied. The message is already in the transcript; this only asks for a turn.
/// </summary>
public class NegotiationWakeJob(
    IServiceScopeFactory scopeFactory,
    ILogger<NegotiationWakeJob> logger)
{
    [AutomaticRetry(Attempts = 0)]
    public async Task RunAsync(Guid tenantId, Guid negotiationId, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var tenantUow = scope.ServiceProvider.GetRequiredService<ITenantUnitOfWork>();
        await tenantUow.SetCurrentTenantByIdAsync(tenantId);

        // Hangfire bypasses the MediatR pipeline, so [RequiresFeature] is inert here.
        var featureService = scope.ServiceProvider.GetRequiredService<IFeatureService>();
        if (!await featureService.IsFeatureEnabledAsync(tenantId, TenantFeature.AIRateNegotiation))
        {
            return;
        }

        try
        {
            var starter = scope.ServiceProvider.GetRequiredService<INegotiationTurnStarter>();
            await starter.TryWakeAsync(negotiationId, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not wake the dispatch agent for negotiation {NegotiationId}",
                negotiationId);
        }
    }
}

public class HangfireNegotiationWakeRunner(IBackgroundJobClient jobClient)
    : IDelayedBackgroundJobRunner<NegotiationWakeRequest>
{
    public void Schedule(NegotiationWakeRequest request, TimeSpan delay)
    {
        jobClient.Schedule<NegotiationWakeJob>(
            job => job.RunAsync(request.TenantId, request.NegotiationId, CancellationToken.None),
            delay);
    }
}
