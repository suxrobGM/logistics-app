using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives;
using Logistics.Domain.Primitives.Enums;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Infrastructure.Services;

internal sealed class AiQuotaService(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow) : IAiQuotaService
{
    public async Task<AiQuotaStatus> GetQuotaStatusAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenantInfo = await GetTenantQuotaInfoAsync(tenantId, ct);

        // Unlimited quota (non-subscription tenants or plans without quota)
        if (tenantInfo is null)
            return new AiQuotaStatus(0, 0, 0, IsOverQuota: false);

        var (quota, planName, quotaResetAt) = tenantInfo.Value;

        // If tenant has a quota reset this week, count from that date; otherwise use ISO week start
        var weekStart = DateTimeHelpers.GetCurrentIsoWeekStart();
        var countFrom = quotaResetAt > weekStart ? quotaResetAt.Value : weekStart;

        var usedThisWeek = await tenantUow.Repository<DispatchSession>().Query()
            .CountAsync(s =>
                s.StartedAt >= countFrom &&
                (s.Status == DispatchSessionStatus.Running ||
                 s.Status == DispatchSessionStatus.Completed), ct);

        var remaining = Math.Max(0, quota - usedThisWeek);
        var resetsAt = countFrom.AddDays(7);

        return new AiQuotaStatus(
            WeeklyQuota: quota,
            UsedThisWeek: usedThisWeek,
            Remaining: remaining,
            IsOverQuota: usedThisWeek >= quota,
            PlanName: planName,
            ResetsAt: resetsAt);
    }

    private async Task<(int Quota, string? PlanName, DateTime? QuotaResetAt)?> GetTenantQuotaInfoAsync(
        Guid tenantId, CancellationToken ct)
    {
        var tenant = await masterUow.Repository<Tenant>().GetByIdAsync(tenantId, ct);

        if (tenant is null || tenant.Subscription is null)
            return null;

        var plan = await masterUow.Repository<SubscriptionPlan>()
            .GetByIdAsync(tenant.Subscription.PlanId, ct);

        if (plan?.WeeklyAiSessionQuota is null)
            return null;

        return (plan.WeeklyAiSessionQuota.Value, plan.Name, tenant.QuotaResetAt);
    }

}
