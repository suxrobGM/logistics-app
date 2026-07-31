using Logistics.Application.Abstractions.Payments.Stripe;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
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

        // Only an unknown tenant reports nothing - an unsubscribed one still has spend to show.
        if (tenantInfo is null)
            return new AIQuotaStatus(0m, 0m, IsOverQuota: false);

        var (budget, planName, quotaResetAt, blockOverage, overageBillable) = tenantInfo;

        // If tenant has a quota reset this week, count from that date; otherwise use ISO week start
        var weekStart = DateTimeHelpers.GetCurrentIsoWeekStart();
        var countFrom = quotaResetAt > weekStart ? quotaResetAt.Value : weekStart;

        // Every status counts - failed and cancelled runs still consumed paid tokens.
        var spentThisWeek = await tenantUow.Repository<AgentSession>().Query()
            .Where(s => s.StartedAt >= countFrom)
            .SumAsync(s => s.EstimatedCostUsd, ct);

        var resetsAt = countFrom.AddDays(7);
        var isOverQuota = spentThisWeek >= budget;

        // Zero spend means nothing to bill: IsOverage is stamped only once spend passed the budget
        // in this same window, and spend only grows within one.
        var overageChargesUsd = spentThisWeek > 0
            ? await SumOverageChargesAsync(countFrom, ct)
            : 0m;

        return new AIQuotaStatus(budget, spentThisWeek, isOverQuota)
        {
            PlanName = planName,
            ResetsAt = resetsAt,
            OverageChargesUsd = overageChargesUsd,
            OverageBlocked = blockOverage && isOverQuota,
            OverageBillable = overageBillable
        };
    }

    /// <summary>
    /// Runs even when the tenant is under quota: gating it on IsOverQuota looks free but zeroes the
    /// figure after a mid-week plan upgrade, since only ResetTenantQuotas moves countFrom.
    /// </summary>
    private async Task<decimal> SumOverageChargesAsync(DateTime countFrom, CancellationToken ct)
    {
        // Units round per session in memory - the min-1 ceiling doesn't translate to SQL, and
        // summing raw cost first would drift from the invoice.
        var overageCosts = await tenantUow.Repository<AgentSession>().Query()
            .Where(s => s.StartedAt >= countFrom)
            .Where(AIOverageBilling.Billable)
            .Select(s => s.EstimatedCostUsd)
            .ToListAsync(ct);

        return overageCosts.Sum(AIOverageBilling.UnitsFor) * AIOverageBilling.UnitUsd;
    }

    /// <summary>Null only for an unknown tenant; the rest fall back per <see cref="AIWeeklyBudget"/>.</summary>
    private async Task<TenantQuotaInfo?> GetTenantQuotaInfoAsync(
        Guid tenantId, CancellationToken ct)
    {
        var tenant = await masterUow.Repository<Tenant>().GetByIdAsync(tenantId, ct);

        if (tenant is null)
            return null;

        var planRepo = masterUow.Repository<SubscriptionPlan>();
        var plan = tenant.Subscription is not null
            ? await planRepo.GetByIdAsync(tenant.Subscription.PlanId, ct)
            : null;

        // Only the unsubscribed path pays for the catalogue read; this runs on every AI request.
        var budget = plan?.WeeklyAIBudgetUsd
                     ?? AIWeeklyBudget.FallbackFrom(await planRepo.GetListAsync(ct: ct));

        return new TenantQuotaInfo(
            BudgetUsd: budget,
            PlanName: plan?.Name,
            QuotaResetAt: tenant.QuotaResetAt,
            BlockOverage: tenant.Settings?.BlockAIOverage ?? false,
            OverageBillable: AIOverageBilling.CanBill(tenant));
    }

    #region Internal records

    public record TenantQuotaInfo(
        decimal BudgetUsd,
        string? PlanName,
        DateTime? QuotaResetAt,
        bool BlockOverage,
        bool OverageBillable);

    #endregion
}
