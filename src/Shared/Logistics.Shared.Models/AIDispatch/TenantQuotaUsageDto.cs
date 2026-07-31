namespace Logistics.Shared.Models;

public record TenantQuotaUsageDto
{
    public Guid TenantId { get; set; }
    public string? TenantName { get; set; }
    public string? CompanyName { get; set; }
    public string? PlanName { get; set; }
    public decimal WeeklyBudgetUsd { get; set; }
    public decimal SpentThisWeekUsd { get; set; }
    public decimal RemainingUsd { get; set; }
    public bool IsOverQuota { get; set; }

    /// <summary>Raw model cost beyond the budget this week (pre-markup; not what Stripe bills).</summary>
    public decimal OverageUsd { get; set; }
    public DateTime? QuotaResetAt { get; set; }
    public int TotalTokensUsed { get; set; }
    public string? LastModelUsed { get; set; }

    /// <summary>Subscription revenue per month: plan base price + per-truck price × truck count.</summary>
    public decimal MonthlyRevenueUsd { get; set; }

    /// <summary>Estimated LLM spend over the last 30 days, from per-session token accounting.</summary>
    public decimal MonthlyLlmCostUsd { get; set; }

    /// <summary>30-day LLM cost as a percentage of monthly revenue; null when revenue is zero.</summary>
    public decimal? CostToRevenuePercent { get; set; }

    /// <summary>Completed sessions this week that ran beyond quota (billed as overage).</summary>
    public int OverageSessionsThisWeek { get; set; }
}
