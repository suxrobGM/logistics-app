using System.Globalization;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Infrastructure.Services;

internal sealed class AiQuotaService(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow) : IAiQuotaService
{
    public async Task<AiQuotaStatus> GetQuotaStatusAsync(Guid tenantId, CancellationToken ct = default)
    {
        var weeklyQuota = await GetWeeklyQuotaAsync(tenantId, ct);

        // Unlimited quota (non-subscription tenants or plans without quota)
        if (weeklyQuota is null)
            return new AiQuotaStatus(0, 0, 0, IsOverQuota: false);

        var weekStart = GetCurrentIsoWeekStart();
        var usedThisWeek = await tenantUow.Repository<DispatchSession>().Query()
            .CountAsync(s =>
                s.StartedAt >= weekStart &&
                (s.Status == DispatchSessionStatus.Running ||
                 s.Status == DispatchSessionStatus.Completed), ct);

        var remaining = Math.Max(0, weeklyQuota.Value - usedThisWeek);
        return new AiQuotaStatus(
            WeeklyQuota: weeklyQuota.Value,
            UsedThisWeek: usedThisWeek,
            Remaining: remaining,
            IsOverQuota: usedThisWeek >= weeklyQuota.Value);
    }

    private async Task<int?> GetWeeklyQuotaAsync(Guid tenantId, CancellationToken ct)
    {
        var tenant = await masterUow.Repository<Tenant>().GetByIdAsync(tenantId, ct);

        if (tenant is null || !tenant.IsSubscriptionRequired || tenant.Subscription is null)
            return null;

        var plan = await masterUow.Repository<SubscriptionPlan>()
            .GetByIdAsync(tenant.Subscription.PlanId, ct);

        return plan?.WeeklyAiSessionQuota;
    }

    private static DateTime GetCurrentIsoWeekStart()
    {
        var today = DateTime.UtcNow.Date;
        var dayOfWeek = (int)today.DayOfWeek;
        // ISO weeks start on Monday (DayOfWeek.Sunday = 0, Monday = 1)
        var daysToSubtract = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
        return today.AddDays(-daysToSubtract);
    }
}
