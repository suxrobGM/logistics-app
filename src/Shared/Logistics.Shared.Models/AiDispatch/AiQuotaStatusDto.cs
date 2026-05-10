namespace Logistics.Shared.Models;

/// <summary>
/// Tenant-facing quota status. Shows usage as a percentage (no raw numbers).
/// </summary>
public record AiQuotaStatusDto
{
    public double UsagePercent { get; set; }
    public bool IsOverQuota { get; set; }
    public string? PlanName { get; set; }
    public DateTime ResetsAt { get; set; }
    public string? AllowedModelTier { get; set; }
    public string? CurrentModel { get; set; }
}
