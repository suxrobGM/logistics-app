using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Application.Queries;

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
            .GetListAsync(p => planIds.Contains(p.Id) && p.WeeklyAiSessionQuota != null, ct))
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
        var usages = new List<TenantQuotaUsageDto>();

        foreach (var tenant in tenants)
        {
            if (!subscriptionByTenant.TryGetValue(tenant.Id, out var subscription)
                || !plans.TryGetValue(subscription.PlanId, out var plan))
                continue;

            var weeklyQuota = plan.WeeklyAiSessionQuota!.Value;

            var countFrom = tenant.QuotaResetAt > weekStart ? tenant.QuotaResetAt.Value : weekStart;
            int usedThisWeek;
            int totalTokens = 0;
            decimal totalCost = 0;
            string? lastModel = null;

            try
            {
                using var scope = scopeFactory.CreateScope();
                var tenantUow = scope.ServiceProvider.GetRequiredService<ITenantUnitOfWork>();
                tenantUow.SetCurrentTenant(tenant);

                var sessions = await tenantUow.Repository<DispatchSession>().Query()
                    .Where(s =>
                        s.StartedAt >= countFrom &&
                        (s.Status == DispatchSessionStatus.Running ||
                         s.Status == DispatchSessionStatus.Completed))
                    .Select(s => new
                    {
                        s.InputTokensUsed,
                        s.OutputTokensUsed,
                        s.EstimatedCostUsd,
                        s.ModelUsed
                    })
                    .ToListAsync(ct);

                usedThisWeek = sessions.Count;
                totalTokens = sessions.Sum(s => s.InputTokensUsed + s.OutputTokensUsed);
                totalCost = sessions.Sum(s => s.EstimatedCostUsd);
                lastModel = sessions.LastOrDefault()?.ModelUsed;
            }
            catch
            {
                continue;
            }

            var remaining = Math.Max(0, weeklyQuota - usedThisWeek);
            usages.Add(new TenantQuotaUsageDto
            {
                TenantId = tenant.Id,
                TenantName = tenant.Name,
                CompanyName = tenant.CompanyName,
                PlanName = plan.Name,
                WeeklyQuota = weeklyQuota,
                UsedThisWeek = usedThisWeek,
                Remaining = remaining,
                IsOverQuota = usedThisWeek >= weeklyQuota,
                OverageCount = Math.Max(0, usedThisWeek - weeklyQuota),
                QuotaResetAt = tenant.QuotaResetAt,
                TotalTokensUsed = totalTokens,
                TotalEstimatedCostUsd = totalCost,
                LastModelUsed = lastModel
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
}
