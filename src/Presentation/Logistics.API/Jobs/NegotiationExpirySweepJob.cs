using Hangfire;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Abstractions.Features;
using Logistics.Application.Modules.Integrations.Negotiation.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.API.Jobs;

/// <summary>
/// Closes negotiations the broker never answered and revokes their reply addresses. No agent turn:
/// silence is not news, and a dispatcher can reopen the conversation by countering again.
/// </summary>
public class NegotiationExpirySweepJob(
    ILogger<NegotiationExpirySweepJob> logger,
    IServiceScopeFactory scopeFactory)
{
    public static void ScheduleJobs()
    {
        RecurringJob.AddOrUpdate<NegotiationExpirySweepJob>(
            "negotiation-expiry-sweep",
            job => job.ExpireStaleNegotiationsAsync(CancellationToken.None),
            Cron.HourInterval(6));
    }

    [AutomaticRetry(Attempts = 2)]
    public Task ExpireStaleNegotiationsAsync(CancellationToken ct) =>
        TenantJobRunner.ForEachTenantAsync(
            scopeFactory, logger, "negotiation expiry sweep", ExpireForTenantAsync, ct);

    private async Task ExpireForTenantAsync(IServiceScope scope, Tenant tenant, CancellationToken ct)
    {
        var featureService = scope.ServiceProvider.GetRequiredService<IFeatureService>();
        if (!await featureService.IsFeatureEnabledAsync(tenant.Id, TenantFeature.AIRateNegotiation))
        {
            return;
        }

        var tenantUow = scope.ServiceProvider.GetRequiredService<ITenantUnitOfWork>();
        tenantUow.SetCurrentTenant(tenant);

        var stale = await tenantUow.Repository<RateNegotiation>()
            .GetListAsync(RateNegotiation.LapsedAt(DateTime.UtcNow), ct);

        if (stale.Count == 0)
        {
            return;
        }

        // Before the close, not after: a retry re-queries open threads only, so a revocation left
        // until after the status flips would never run on the second attempt.
        var routeRegistry = scope.ServiceProvider.GetRequiredService<IInboundEmailRouteRegistry>();
        await routeRegistry.RevokeAsync(stale.Select(n => n.ReplyToken), ct);

        foreach (var negotiation in stale)
        {
            negotiation.Close(RateNegotiationStatus.Expired, "The broker did not reply before the offer lapsed.");
        }

        await tenantUow.SaveChangesAsync(ct);

        logger.LogInformation(
            "Expired {Count} negotiations for tenant {TenantName}", stale.Count, tenant.Name);

        var broadcastService = scope.ServiceProvider.GetRequiredService<IAIDispatchBroadcastService>();
        var dtos = await NegotiationDtoBatch.MapAsync(tenantUow, stale, ct);

        await Task.WhenAll(dtos.Select(dto => broadcastService.BroadcastNegotiationAsync(tenant.Id, dto)));
    }
}
