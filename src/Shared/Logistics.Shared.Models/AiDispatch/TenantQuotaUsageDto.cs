namespace Logistics.Shared.Models;

public record TenantQuotaUsageDto
{
    public Guid TenantId { get; set; }
    public string? TenantName { get; set; }
    public string? CompanyName { get; set; }
    public string? PlanName { get; set; }
    public int WeeklyQuota { get; set; }
    public int UsedThisWeek { get; set; }
    public int Remaining { get; set; }
    public bool IsOverQuota { get; set; }
    public int OverageCount { get; set; }
    public DateTime? QuotaResetAt { get; set; }
    public int TotalTokensUsed { get; set; }
    public decimal TotalEstimatedCostUsd { get; set; }
    public string? LastModelUsed { get; set; }
}
