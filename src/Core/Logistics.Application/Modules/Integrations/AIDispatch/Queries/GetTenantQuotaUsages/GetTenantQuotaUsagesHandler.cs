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

    public async Task<PagedResult<TenantQuotaUsageDto>> Handle(
        GetTenantQuotaUsagesQuery request, CancellationToken ct)
    {
        // Query subscriptions to find tenants with plans that have AI quotas
        var subscriptions = await masterUow.Repository<Subscription>()
            .GetListAsync(ct: ct);

        var planIds = subscriptions.Select(s => s.PlanId).Distinct().ToList();
        var plans = (await masterUow.Repository<SubscriptionPlan>()
            .GetListAsync(p => planIds.Contains(p.Id) && p.WeeklyAIRequestQuota != null, ct))
            .ToDictionary(p => p.Id);

        // Only fetch tenants that have plans with AI quotas
        var tenantIds = subscriptions
            .Where(s => plans.ContainsKey(s.PlanId))
            .Select(s => s.TenantId)
            .ToList();

        var tenants = await masterUow.Repository<Tenant>()
            .GetListAsync(t => tenantIds.Contains(t.Id) && t.ConnectionString != null, ct);

        var subscriptionByTenant = subscriptions.ToDictionary(s => s.TenantId);

        var weekStart = DateTimeHelpers.GetCurrentIsoWeekStart();
        var costWindowStart = DateTime.UtcNow.AddDays(-30);

        var targets = tenants
            .Select(t => subscriptionByTenant.TryGetValue(t.Id, out var subscription)
                         && plans.TryGetValue(subscription.PlanId, out var plan)
                ? new { Tenant = t, Subscription = subscription, Plan = plan }
                : null)
            .Where(x => x is not null)
            .ToList();

        // The default sort is a computed column, so every tenant has to be read before the page can
        // be picked. Each read opens its own scope and DbContext, so they are safe to overlap;
        // the cap keeps a large tenant list from exhausting the connection pool.
        var results = new TenantQuotaUsageDto?[targets.Count];
        await Parallel.ForEachAsync(
            Enumerable.Range(0, targets.Count),
            new ParallelOptions { MaxDegreeOfParallelism = MaxTenantReadConcurrency, CancellationToken = ct },
            async (index, token) =>
            {
                var (tenant, subscription, plan) =
                    (targets[index]!.Tenant, targets[index]!.Subscription, targets[index]!.Plan);

                var countFrom = tenant.QuotaResetAt > weekStart ? tenant.QuotaResetAt.Value : weekStart;
                var usage = await ReadTenantUsageAsync(tenant, countFrom, costWindowStart, token);
                if (usage is null)
                {
                    return;
                }

                var weeklyQuota = plan.WeeklyAIRequestQuota!.Value;
                var monthlyRevenue = MonthlyRevenueOf(plan, subscription, usage.TruckCount);
                results[index] = new TenantQuotaUsageDto
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
            });

        var usages = results.Where(x => x is not null).Select(x => x!).ToList();

        // Apply sorting and pagination using existing extensions
        var totalItems = usages.Count;
        var paged = usages.AsQueryable()
            .OrderBy(request.OrderBy ?? "-UsedThisWeek")
            .ApplyPaging(request.Page, request.PageSize)
            .ToArray();

        return PagedResult<TenantQuotaUsageDto>.Ok(paged, totalItems, request.PageSize);
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

    private sealed record TenantUsage(
        int UsedThisWeek,
        int OverageSessions,
        int TotalTokens,
        decimal TotalCost,
        decimal MonthlyLlmCost,
        int TruckCount,
        string? LastModel);

    /// <summary>
    /// Reads one tenant's session aggregates from its own database. Returns null when the tenant
    /// database is unreachable, so a single bad connection string cannot fail the whole listing.
    /// </summary>
    /// <remarks>
    /// The aggregation runs in the database rather than over materialised rows: a busy tenant can
    /// have tens of thousands of sessions in the 30-day cost window, and every one of them would
    /// otherwise cross the wire only to be summed and discarded.
    /// </remarks>
    private async Task<TenantUsage?> ReadTenantUsageAsync(
        Tenant tenant, DateTime countFrom, DateTime costWindowStart, CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var tenantUow = scope.ServiceProvider.GetRequiredService<ITenantUnitOfWork>();
            tenantUow.SetCurrentTenant(tenant);

            // Cost window is 30 days (margin vs monthly revenue); quota window stays weekly, so the
            // week-scoped figures are conditional sums over the same one-pass scan.
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

            var truckCount = await tenantUow.Repository<Truck>().CountAsync(ct: ct);

            // No sessions in the window at all - the tenant still belongs in the report.
            if (totals is null)
            {
                return new TenantUsage(0, 0, 0, 0m, 0m, truckCount, null);
            }

            return new TenantUsage(
                UsedThisWeek: totals.UsedThisWeek,
                OverageSessions: totals.OverageSessions,
                TotalTokens: totals.TotalTokens,
                TotalCost: totals.TotalCost,
                MonthlyLlmCost: totals.MonthlyLlmCost,
                TruckCount: truckCount,
                LastModel: lastModel);
        }
        catch
        {
            return null;
        }
    }
}
