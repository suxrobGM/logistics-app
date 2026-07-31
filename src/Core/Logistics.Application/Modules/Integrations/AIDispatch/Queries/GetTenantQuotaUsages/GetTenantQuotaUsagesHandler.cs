using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.AIDispatch;
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

    private sealed record QuotaTenant(
        Tenant Tenant,
        Subscription? Subscription,
        SubscriptionPlan? Plan,
        decimal WeeklyBudgetUsd);

    private sealed record TenantUsage(
        decimal SpentThisWeekUsd,
        int OverageSessions,
        int TotalTokens,
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
            .OrderBy(request.OrderBy ?? "-SpentThisWeekUsd")
            .ApplyPaging(request.Page, request.PageSize)
            .ToArray();

        return PagedResult<TenantQuotaUsageDto>.Ok(paged, usages.Count, request.PageSize);
    }

    /// <summary>
    /// Every tenant whose database is reachable, subscribed or not - an unsubscribed one burns
    /// model cost too, and is metered against the fallback budget (<see cref="AIWeeklyBudget"/>).
    /// Ordered by id so that rows tying on the sort column - which many do, since idle tenants all
    /// sit at zero - land on the same page from one request to the next.
    /// </summary>
    private async Task<List<QuotaTenant>> LoadQuotaTenantsAsync(CancellationToken ct)
    {
        var tenants = await masterUow.Repository<Tenant>()
            .GetListAsync(t => t.ConnectionString != null, ct);

        var subscriptions = (await masterUow.Repository<Subscription>().GetListAsync(ct: ct))
            .DistinctBy(s => s.TenantId)
            .ToDictionary(s => s.TenantId);

        var plans = await masterUow.Repository<SubscriptionPlan>().GetListAsync(ct: ct);
        var plansById = plans.ToDictionary(p => p.Id);
        var fallbackBudget = AIWeeklyBudget.FallbackFrom(plans);

        return tenants
            .Select(t =>
            {
                var subscription = subscriptions.GetValueOrDefault(t.Id);
                var plan = subscription is not null ? plansById.GetValueOrDefault(subscription.PlanId) : null;
                return new QuotaTenant(t, subscription, plan, plan?.WeeklyAIBudgetUsd ?? fallbackBudget);
            })
            .OrderBy(x => x.Tenant.Id)
            .ToList();
    }

    /// <summary>
    /// Builds one report row, or null when the tenant's usage could not be read.
    /// </summary>
    private async Task<TenantQuotaUsageDto?> BuildUsageRowAsync(
        QuotaTenant target, DateTime weekStart, DateTime costWindowStart, CancellationToken ct)
    {
        var (tenant, subscription, plan, weeklyBudget) = target;

        var countFrom = tenant.QuotaResetAt > weekStart ? tenant.QuotaResetAt.Value : weekStart;
        var usage = await ReadTenantUsageAsync(tenant, countFrom, costWindowStart, ct);
        if (usage is null)
        {
            return null;
        }

        var monthlyRevenue = plan is not null && subscription is not null
            ? MonthlyRevenueOf(plan, subscription, usage.TruckCount)
            : 0m;

        return new TenantQuotaUsageDto
        {
            TenantId = tenant.Id,
            TenantName = tenant.Name,
            CompanyName = tenant.CompanyName,
            PlanName = plan?.Name,
            WeeklyBudgetUsd = weeklyBudget,
            SpentThisWeekUsd = usage.SpentThisWeekUsd,
            RemainingUsd = Math.Max(0m, weeklyBudget - usage.SpentThisWeekUsd),
            IsOverQuota = usage.SpentThisWeekUsd >= weeklyBudget,
            OverageUsd = Math.Max(0m, usage.SpentThisWeekUsd - weeklyBudget),
            QuotaResetAt = tenant.QuotaResetAt,
            TotalTokensUsed = usage.TotalTokens,
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
                .Where(s => s.StartedAt >= costWindowStart);

            // Spend counts every status, matching AIQuotaService - this admin view and the
            // tenant's own quota endpoint must not disagree. Overage stays Completed-only (metered).
            var totals = await sessions
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    SpentThisWeekUsd = g.Sum(s =>
                        s.StartedAt >= countFrom ? s.EstimatedCostUsd : 0m),
                    OverageSessions = g.Sum(s =>
                        s.StartedAt >= countFrom && s.Status == AgentSessionStatus.Completed && s.IsOverage
                            ? 1
                            : 0),
                    TotalTokens = g.Sum(s =>
                        s.StartedAt >= countFrom ? s.InputTokensUsed + s.OutputTokensUsed : 0),
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
                SpentThisWeekUsd: totals?.SpentThisWeekUsd ?? 0m,
                OverageSessions: totals?.OverageSessions ?? 0,
                TotalTokens: totals?.TotalTokens ?? 0,
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
