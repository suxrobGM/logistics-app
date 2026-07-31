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

        // Unlimited budget (non-subscription tenants or plans without a budget)
        if (tenantInfo is null)
            return new AIQuotaStatus(0m, 0m, IsOverQuota: false);

        var (budget, planName, quotaResetAt, blockOverage) = tenantInfo;

        // If tenant has a quota reset this week, count from that date; otherwise use ISO week start
        var weekStart = DateTimeHelpers.GetCurrentIsoWeekStart();
        var countFrom = quotaResetAt > weekStart ? quotaResetAt.Value : weekStart;

        // Every status counts - failed and cancelled runs still consumed paid tokens.
        var spentThisWeek = await tenantUow.Repository<AgentSession>().Query()
            .Where(s => s.StartedAt >= countFrom)
            .SumAsync(s => s.EstimatedCostUsd, ct);

        // Units round per session in memory - the min-1 ceiling doesn't translate to SQL, and
        // summing raw cost first would drift from the invoice.
        // Runs unconditionally on purpose: gating it on IsOverQuota looks free (spend only grows
        // within a window) but zeroes the figure after a mid-week plan upgrade - the one moment a
        // tenant most wants it, since only ResetTenantQuotas moves countFrom, not a plan change.
        var overageCosts = await tenantUow.Repository<AgentSession>().Query()
            .Where(s => s.StartedAt >= countFrom)
            .Where(AIOverageBilling.Billable)
            .Select(s => s.EstimatedCostUsd)
            .ToListAsync(ct);
        var overageChargesUsd = overageCosts.Sum(AIOverageBilling.UnitsFor) * AIOverageBilling.UnitUsd;

        var resetsAt = countFrom.AddDays(7);
        var isOverQuota = spentThisWeek >= budget;

        return new AIQuotaStatus(budget, spentThisWeek, isOverQuota)
        {
            PlanName = planName,
            ResetsAt = resetsAt,
            OverageChargesUsd = overageChargesUsd,
            OverageBlocked = blockOverage && isOverQuota
        };
    }

    private async Task<TenantQuotaInfo?> GetTenantQuotaInfoAsync(
        Guid tenantId, CancellationToken ct)
    {
        var tenant = await masterUow.Repository<Tenant>().GetByIdAsync(tenantId, ct);

        if (tenant is null || tenant.Subscription is null)
            return null;

        var plan = await masterUow.Repository<SubscriptionPlan>()
            .GetByIdAsync(tenant.Subscription.PlanId, ct);

        if (plan?.WeeklyAIBudgetUsd is null)
            return null;

        return new TenantQuotaInfo(
            BudgetUsd: plan.WeeklyAIBudgetUsd.Value,
            PlanName: plan.Name,
            QuotaResetAt: tenant.QuotaResetAt,
            BlockOverage: tenant.Settings?.BlockAIOverage ?? false);
    }

    #region Internal records

    public record TenantQuotaInfo(
        decimal BudgetUsd,
        string? PlanName,
        DateTime? QuotaResetAt,
        bool BlockOverage);

    #endregion
}
