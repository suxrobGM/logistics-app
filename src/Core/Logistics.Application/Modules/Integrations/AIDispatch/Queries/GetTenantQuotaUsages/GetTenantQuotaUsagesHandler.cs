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
        var usages = new List<TenantQuotaUsageDto>();

        foreach (var tenant in tenants)
        {
            if (!subscriptionByTenant.TryGetValue(tenant.Id, out var subscription)
                || !plans.TryGetValue(subscription.PlanId, out var plan))
            {
                continue;
            }

            var countFrom = tenant.QuotaResetAt > weekStart ? tenant.QuotaResetAt.Value : weekStart;
            var usage = await ReadTenantUsageAsync(tenant, countFrom, costWindowStart, ct);
            if (usage is null)
            {
                continue;
            }

            var weeklyQuota = plan.WeeklyAIRequestQuota!.Value;
            var monthlyRevenue = plan.Price.Amount + plan.PerTruckPrice.Amount * usage.TruckCount;
            usages.Add(new TenantQuotaUsageDto
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
            });
        }

        // Apply sorting and pagination using existing extensions
        var totalItems = usages.Count;
        var paged = usages.AsQueryable()
            .OrderBy(request.OrderBy ?? "-UsedThisWeek")
            .ApplyPaging(request.Page, request.PageSize)
            .ToArray();

        return PagedResult<TenantQuotaUsageDto>.Ok(paged, totalItems, request.PageSize);
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
    private async Task<TenantUsage?> ReadTenantUsageAsync(
        Tenant tenant, DateTime countFrom, DateTime costWindowStart, CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var tenantUow = scope.ServiceProvider.GetRequiredService<ITenantUnitOfWork>();
            tenantUow.SetCurrentTenant(tenant);

            // Cost window is 30 days (margin vs monthly revenue); quota window stays weekly.
            var sessions = await tenantUow.Repository<AgentSession>().Query()
                .Where(s =>
                    s.StartedAt >= costWindowStart &&
                    (s.Status == AgentSessionStatus.Running ||
                     s.Status == AgentSessionStatus.Completed))
                .Select(s => new
                {
                    s.StartedAt,
                    s.InputTokensUsed,
                    s.OutputTokensUsed,
                    s.EstimatedCostUsd,
                    s.ModelUsed,
                    s.RequestCost,
                    s.IsOverage,
                    s.Status
                })
                .ToListAsync(ct);

            var weekSessions = sessions.Where(s => s.StartedAt >= countFrom).ToList();

            return new TenantUsage(
                UsedThisWeek: weekSessions.Sum(s => s.RequestCost),
                OverageSessions: weekSessions.Count(s => s.IsOverage && s.Status == AgentSessionStatus.Completed),
                TotalTokens: weekSessions.Sum(s => s.InputTokensUsed + s.OutputTokensUsed),
                TotalCost: weekSessions.Sum(s => s.EstimatedCostUsd),
                MonthlyLlmCost: sessions.Sum(s => s.EstimatedCostUsd),
                TruckCount: await tenantUow.Repository<Truck>().CountAsync(ct: ct),
                LastModel: weekSessions.MaxBy(s => s.StartedAt)?.ModelUsed);
        }
        catch
        {
            return null;
        }
    }
}
