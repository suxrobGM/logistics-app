using Hangfire;
using Logistics.Application.Abstractions.Context;
using Logistics.Application.Modules.AI.Negotiation.Services;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.API.Jobs;

/// <summary>Starts a dispatch agent turn after a broker reply. The reply is already saved in the transcript; this job only requests the turn.</summary>
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

        // Hangfire bypasses the request pipeline, so [RequiresFeature] is inert here.
        var featureService = scope.ServiceProvider.GetRequiredService<IFeatureService>();
        if (!await featureService.IsFeatureEnabledAsync(tenantId, TenantFeature.AINegotiation))
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
            throw;
        }
    }
}
