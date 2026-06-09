namespace Logistics.Shared.Models;

/// <summary>
/// A subscription plan's weekly AI dispatch quota, editable by an admin.
/// A null <see cref="WeeklyAiRequestQuota"/> means unlimited.
/// </summary>
public record PlanQuotaDto
{
    public Guid PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public int? WeeklyAiRequestQuota { get; set; }
}
