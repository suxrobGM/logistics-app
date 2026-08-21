using Hangfire;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Abstractions.Features;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;

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
            Cron.Hourly());
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

        var now = DateTime.UtcNow;
        var stale = await tenantUow.Repository<RateNegotiation>().GetListAsync(
            n => n.Status == RateNegotiationStatus.AwaitingBroker && n.ExpiresAt != null && n.ExpiresAt < now,
            ct);

        if (stale.Count == 0)
        {
            return;
        }

        foreach (var negotiation in stale)
        {
            negotiation.Close(RateNegotiationStatus.Expired, "The broker did not reply before the offer lapsed.");
        }

        await tenantUow.SaveChangesAsync(ct);
        await RevokeRoutesAsync(scope, stale, ct);

        logger.LogInformation(
            "Expired {Count} negotiations for tenant {TenantName}", stale.Count, tenant.Name);

        var broadcastService = scope.ServiceProvider.GetRequiredService<IAIDispatchBroadcastService>();
        foreach (var negotiation in stale)
        {
            await broadcastService.BroadcastNegotiationAsync(tenant.Id, negotiation.ToDto());
        }
    }

    /// <summary>
    /// The reply address outlives the thread unless it is revoked here, and an address that still
    /// routes is an open door into a tenant's inbox.
    /// </summary>
    private static async Task RevokeRoutesAsync(
        IServiceScope scope, List<RateNegotiation> expired, CancellationToken ct)
    {
        var masterUow = scope.ServiceProvider.GetRequiredService<IMasterUnitOfWork>();
        var tokens = expired.Select(n => n.ReplyToken).ToArray();

        var routes = await masterUow.Repository<InboundEmailRoute>()
            .GetListAsync(r => tokens.Contains(r.ThreadToken) && r.RevokedAt == null, ct);

        if (routes.Count == 0)
        {
            return;
        }

        foreach (var route in routes)
        {
            route.RevokedAt = DateTime.UtcNow;
        }

        await masterUow.SaveChangesAsync(ct);
    }
}
