namespace Logistics.Shared.Models;

/// <summary>
/// A subscription plan's weekly AI budget in USD of estimated model cost, editable by an admin.
/// A null <see cref="WeeklyAIBudgetUsd"/> means unlimited.
/// </summary>
public record PlanQuotaDto
{
    public Guid PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public decimal? WeeklyAIBudgetUsd { get; set; }
}
