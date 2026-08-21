using Hangfire;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Abstractions.Features;
using Logistics.Application.Modules.Integrations.Negotiation.Services;
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

        var routeRegistry = scope.ServiceProvider.GetRequiredService<IInboundEmailRouteRegistry>();
        await routeRegistry.RevokeAsync(stale.Select(n => n.ReplyToken), ct);

        logger.LogInformation(
            "Expired {Count} negotiations for tenant {TenantName}", stale.Count, tenant.Name);

        var broadcastService = scope.ServiceProvider.GetRequiredService<IAIDispatchBroadcastService>();
        var listings = await GetListingsAsync(tenantUow, stale, ct);

        await Task.WhenAll(stale.Select(n => broadcastService.BroadcastNegotiationAsync(
            tenant.Id, n.ToDto(listings.GetValueOrDefault(n.LoadBoardListingId)))));
    }

    /// <summary>One query for the batch - the listing navigation would lazy-load per row.</summary>
    private static async Task<Dictionary<Guid, LoadBoardListing>> GetListingsAsync(
        ITenantUnitOfWork tenantUow, List<RateNegotiation> negotiations, CancellationToken ct)
    {
        var ids = negotiations.Select(n => n.LoadBoardListingId).Distinct().ToArray();

        var listings = await tenantUow.Repository<LoadBoardListing>()
            .GetListAsync(l => ids.Contains(l.Id), ct);

        return listings.ToDictionary(l => l.Id);
    }
}
