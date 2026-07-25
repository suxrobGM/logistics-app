namespace Logistics.Shared.Models;

/// <summary>
/// A subscription plan's weekly AI dispatch quota, editable by an admin.
/// A null <see cref="WeeklyAIRequestQuota"/> means unlimited.
/// </summary>
public record PlanQuotaDto
{
    public Guid PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public int? WeeklyAIRequestQuota { get; set; }
}
