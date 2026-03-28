namespace Logistics.Shared.Models;

/// <summary>
/// Admin-facing quota status with raw numbers for the tenant quotas management page.
/// </summary>
public record AiQuotaStatusAdminDto
{
    public int WeeklyQuota { get; set; }
    public int UsedThisWeek { get; set; }
    public int Remaining { get; set; }
    public bool IsOverQuota { get; set; }
    public string? PlanName { get; set; }
    public DateTime ResetsAt { get; set; }
}
