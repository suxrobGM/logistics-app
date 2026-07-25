using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives;
using Logistics.Domain.Primitives.Enums;
using Microsoft.EntityFrameworkCore;
using Logistics.Application.Abstractions.AIDispatch;

namespace Logistics.Infrastructure.Persistence.Services.AIDispatch;

internal sealed class AIQuotaService(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow) : IAIQuotaService
{
    public async Task<AIQuotaStatus> GetQuotaStatusAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenantInfo = await GetTenantQuotaInfoAsync(tenantId, ct);

        // Unlimited quota (non-subscription tenants or plans without quota)
        if (tenantInfo is null)
            return new AIQuotaStatus(0, 0, 0, IsOverQuota: false);

        var (quota, planName, quotaResetAt) = tenantInfo;

        // If tenant has a quota reset this week, count from that date; otherwise use ISO week start
        var weekStart = DateTimeHelpers.GetCurrentIsoWeekStart();
        var countFrom = quotaResetAt > weekStart ? quotaResetAt.Value : weekStart;

        var usedThisWeek = await tenantUow.Repository<AIDispatchSession>().Query()
            .Where(s => s.StartedAt >= countFrom && s.Status == AIDispatchSessionStatus.Completed)
            .SumAsync(s => s.RequestCost, ct);

        var remaining = Math.Max(0, quota - usedThisWeek);
        var resetsAt = countFrom.AddDays(7);

        return new AIQuotaStatus(
            WeeklyQuota: quota,
            UsedThisWeek: usedThisWeek,
            Remaining: remaining,
            IsOverQuota: usedThisWeek >= quota,
            PlanName: planName,
            ResetsAt: resetsAt);
    }

    private async Task<TenantQuotaInfo?> GetTenantQuotaInfoAsync(
        Guid tenantId, CancellationToken ct)
    {
        var tenant = await masterUow.Repository<Tenant>().GetByIdAsync(tenantId, ct);

        if (tenant is null || tenant.Subscription is null)
            return null;

        var plan = await masterUow.Repository<SubscriptionPlan>()
            .GetByIdAsync(tenant.Subscription.PlanId, ct);

        if (plan?.WeeklyAIRequestQuota is null)
            return null;

        return new TenantQuotaInfo(
            Quota: plan.WeeklyAIRequestQuota.Value,
            PlanName: plan.Name,
            QuotaResetAt: tenant.QuotaResetAt);
    }

    #region Internal records

    public record TenantQuotaInfo(
        int Quota,
        string? PlanName,
        DateTime? QuotaResetAt);

    #endregion
}
