using Hangfire;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Abstractions.BackgroundJobs;
using Logistics.Application.Abstractions.Features;
using Logistics.Application.Modules.Integrations.AIDispatch.Services;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.API.Jobs;

/// <summary>The alert is already in the transcript; this only asks for a turn.</summary>
public class TripAlertWakeJob(
    IServiceScopeFactory scopeFactory,
    ILogger<TripAlertWakeJob> logger)
{
    [AutomaticRetry(Attempts = 0)]
    public async Task RunAsync(Guid tenantId, Guid tripId, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var tenantUow = scope.ServiceProvider.GetRequiredService<ITenantUnitOfWork>();
        await tenantUow.SetCurrentTenantByIdAsync(tenantId);

        // Hangfire bypasses the request pipeline, so [RequiresFeature] is inert here.
        var featureService = scope.ServiceProvider.GetRequiredService<IFeatureService>();
        if (!await featureService.IsFeatureEnabledAsync(tenantId, TenantFeature.AITripAlerts))
        {
            return;
        }

        try
        {
            var starter = scope.ServiceProvider.GetRequiredService<ITripAlertTurnStarter>();
            await starter.TryWakeAsync(tripId, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not wake the dispatch agent for trip alert {TripId}", tripId);
            throw;
        }
    }
}

public class HangfireTripAlertWakeRunner(IBackgroundJobClient jobClient)
    : IDelayedBackgroundJobRunner<TripAlertWakeRequest>
{
    public void Schedule(TripAlertWakeRequest request, TimeSpan delay)
    {
        jobClient.Schedule<TripAlertWakeJob>(
            job => job.RunAsync(request.TenantId, request.TripId, CancellationToken.None),
            delay);
    }
}
