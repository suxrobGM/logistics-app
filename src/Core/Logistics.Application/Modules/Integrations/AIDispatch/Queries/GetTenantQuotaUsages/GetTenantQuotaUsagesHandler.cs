using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Queries;

internal sealed class GetTenantQuotaUsagesHandler(
    IMasterUnitOfWork masterUow,
    IServiceScopeFactory scopeFactory) : IAppRequestHandler<GetTenantQuotaUsagesQuery, PagedResult<TenantQuotaUsageDto>>
{
    /// <summary>
    /// How many tenant databases this report reads at once. Each read holds a pooled connection
    /// for the duration, so this bounds the report's share of the pool.
    /// </summary>
    private const int MaxTenantReadConcurrency = 8;

    /// <summary>Cost window, chosen to line up with the monthly revenue it is compared against.</summary>
    private static readonly TimeSpan CostWindow = TimeSpan.FromDays(30);

    private sealed record QuotaTenant(Tenant Tenant, Subscription Subscription, SubscriptionPlan Plan);

    private sealed record TenantUsage(
        int UsedThisWeek,
        int OverageSessions,
        int TotalTokens,
        decimal TotalCost,
        decimal MonthlyLlmCost,
        int TruckCount,
        string? LastModel);

    public async Task<PagedResult<TenantQuotaUsageDto>> Handle(
        GetTenantQuotaUsagesQuery request, CancellationToken ct)
    {
        var targets = await LoadQuotaTenantsAsync(ct);
        var weekStart = DateTimeHelpers.GetCurrentIsoWeekStart();
        var costWindowStart = DateTime.UtcNow - CostWindow;

        // The default sort is a computed column, so every tenant has to be read before the page can
        // be picked. Each read opens its own scope and DbContext, so they are safe to overlap; the
        // cap keeps a large tenant list from exhausting the connection pool. Writing into a slot
        // per target keeps the output order stable, which pagination needs when rows tie.
        var rows = new TenantQuotaUsageDto?[targets.Count];
        await Parallel.ForEachAsync(
            targets.Index(),
            new ParallelOptions { MaxDegreeOfParallelism = MaxTenantReadConcurrency, CancellationToken = ct },
            async (entry, token) =>
            {
                rows[entry.Index] = await BuildUsageRowAsync(entry.Item, weekStart, costWindowStart, token);
            });

        var usages = rows.OfType<TenantQuotaUsageDto>().ToList();

        var paged = usages.AsQueryable()
            .OrderBy(request.OrderBy ?? "-UsedThisWeek")
            .ApplyPaging(request.Page, request.PageSize)
            .ToArray();

        return PagedResult<TenantQuotaUsageDto>.Ok(paged, usages.Count, request.PageSize);
    }

    /// <summary>
    /// The tenants this report covers: those on a plan that defines a weekly AI quota and whose
    /// database is reachable. Ordered by id so that rows tying on the sort column - which many do,
    /// since idle tenants all sit at zero - land on the same page from one request to the next.
    /// </summary>
    private async Task<List<QuotaTenant>> LoadQuotaTenantsAsync(CancellationToken ct)
    {
        var subscriptions = await masterUow.Repository<Subscription>().GetListAsync(ct: ct);

        var planIds = subscriptions.Select(s => s.PlanId).Distinct().ToList();
        var plans = (await masterUow.Repository<SubscriptionPlan>()
                .GetListAsync(p => planIds.Contains(p.Id) && p.WeeklyAIRequestQuota != null, ct))
            .ToDictionary(p => p.Id);

        var quotaSubscriptions = subscriptions.Where(s => plans.ContainsKey(s.PlanId)).ToList();
        var tenantIds = quotaSubscriptions.Select(s => s.TenantId).ToList();

        var tenants = (await masterUow.Repository<Tenant>()
                .GetListAsync(t => tenantIds.Contains(t.Id) && t.ConnectionString != null, ct))
            .ToDictionary(t => t.Id);

        return quotaSubscriptions
            .Where(s => tenants.ContainsKey(s.TenantId))
            .Select(s => new QuotaTenant(tenants[s.TenantId], s, plans[s.PlanId]))
            .OrderBy(x => x.Tenant.Id)
            .ToList();
    }

    /// <summary>
    /// Builds one report row, or null when the tenant's usage could not be read.
    /// </summary>
    private async Task<TenantQuotaUsageDto?> BuildUsageRowAsync(
        QuotaTenant target, DateTime weekStart, DateTime costWindowStart, CancellationToken ct)
    {
        var (tenant, subscription, plan) = target;

        var countFrom = tenant.QuotaResetAt > weekStart ? tenant.QuotaResetAt.Value : weekStart;
        var usage = await ReadTenantUsageAsync(tenant, countFrom, costWindowStart, ct);
        if (usage is null)
        {
            return null;
        }

        var weeklyQuota = plan.WeeklyAIRequestQuota!.Value;
        var monthlyRevenue = MonthlyRevenueOf(plan, subscription, usage.TruckCount);

        return new TenantQuotaUsageDto
        {
            TenantId = tenant.Id,
            TenantName = tenant.Name,
            CompanyName = tenant.CompanyName,
            PlanName = plan.Name,
            WeeklyQuota = weeklyQuota,
            UsedThisWeek = usage.UsedThisWeek,
            Remaining = Math.Max(0, weeklyQuota - usage.UsedThisWeek),
            IsOverQuota = usage.UsedThisWeek >= weeklyQuota,
            OverageCount = Math.Max(0, usage.UsedThisWeek - weeklyQuota),
            QuotaResetAt = tenant.QuotaResetAt,
            TotalTokensUsed = usage.TotalTokens,
            TotalEstimatedCostUsd = usage.TotalCost,
            LastModelUsed = usage.LastModel,
            MonthlyRevenueUsd = monthlyRevenue,
            MonthlyLlmCostUsd = usage.MonthlyLlmCost,
            CostToRevenuePercent = monthlyRevenue > 0
                ? Math.Round(usage.MonthlyLlmCost / monthlyRevenue * 100, 1)
                : null,
            OverageSessionsThisWeek = usage.OverageSessions
        };
    }

    /// <summary>
    /// Subscription revenue normalised to a month, so it can be compared against the 30-day LLM
    /// cost regardless of how the plan bills. A subscription that is not currently paying (trial,
    /// paused, past due) contributes nothing - reporting its list price would show a healthy
    /// margin for a tenant generating cost against no revenue at all.
    /// </summary>
    private static decimal MonthlyRevenueOf(SubscriptionPlan plan, Subscription subscription, int truckCount)
    {
        if (subscription.Status != SubscriptionStatus.Active)
        {
            return 0;
        }

        var perInterval = plan.Price.Amount + plan.PerTruckPrice.Amount * truckCount;
        var monthsPerInterval = plan.Interval switch
        {
            BillingInterval.Year => 12 * plan.IntervalCount,
            _ => plan.IntervalCount
        };

        return monthsPerInterval > 0 ? perInterval / monthsPerInterval : perInterval;
    }

    /// <summary>
    /// Reads one tenant's session aggregates from its own database. Returns null when the tenant
    /// database is unreachable, so a single bad connection string cannot fail the whole listing.
    /// </summary>
    /// <remarks>
    /// The aggregation runs in the database rather than over materialised rows: a busy tenant can
    /// have tens of thousands of sessions in the 30-day cost window, and every one of them would
    /// otherwise cross the wire only to be summed and discarded. The shapes are pinned by
    /// <c>TenantQuotaUsageQueryTranslationTests</c> - EF only rejects an untranslatable aggregate
    /// at execution time.
    /// </remarks>
    private async Task<TenantUsage?> ReadTenantUsageAsync(
        Tenant tenant, DateTime countFrom, DateTime costWindowStart, CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var tenantUow = scope.ServiceProvider.GetRequiredService<ITenantUnitOfWork>();
            tenantUow.SetCurrentTenant(tenant);

            var sessions = tenantUow.Repository<AgentSession>().Query()
                .Where(s =>
                    s.StartedAt >= costWindowStart &&
                    (s.Status == AgentSessionStatus.Running ||
                     s.Status == AgentSessionStatus.Completed));

            // Quota counts completed sessions only, matching AIQuotaService - the tenant's own
            // quota endpoint and this admin view must not disagree. Cost counts in-flight work too.
            var totals = await sessions
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    UsedThisWeek = g.Sum(s =>
                        s.StartedAt >= countFrom && s.Status == AgentSessionStatus.Completed
                            ? s.RequestCost
                            : 0),
                    OverageSessions = g.Sum(s =>
                        s.StartedAt >= countFrom && s.Status == AgentSessionStatus.Completed && s.IsOverage
                            ? 1
                            : 0),
                    TotalTokens = g.Sum(s =>
                        s.StartedAt >= countFrom ? s.InputTokensUsed + s.OutputTokensUsed : 0),
                    TotalCost = g.Sum(s => s.StartedAt >= countFrom ? s.EstimatedCostUsd : 0m),
                    MonthlyLlmCost = g.Sum(s => s.EstimatedCostUsd)
                })
                .FirstOrDefaultAsync(ct);

            var lastModel = await sessions
                .Where(s => s.StartedAt >= countFrom)
                .OrderByDescending(s => s.StartedAt)
                .Select(s => s.ModelUsed)
                .FirstOrDefaultAsync(ct);

            // totals is null only when the tenant ran nothing in the window; it still belongs in
            // the report, with a truck count and therefore revenue.
            return new TenantUsage(
                UsedThisWeek: totals?.UsedThisWeek ?? 0,
                OverageSessions: totals?.OverageSessions ?? 0,
                TotalTokens: totals?.TotalTokens ?? 0,
                TotalCost: totals?.TotalCost ?? 0m,
                MonthlyLlmCost: totals?.MonthlyLlmCost ?? 0m,
                TruckCount: await tenantUow.Repository<Truck>().CountAsync(ct: ct),
                LastModel: lastModel);
        }
        catch
        {
            return null;
        }
    }
}
