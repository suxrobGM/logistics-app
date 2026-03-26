namespace Logistics.Shared.Models;

public record AiQuotaStatusDto
{
    public int WeeklyQuota { get; set; }
    public int UsedThisWeek { get; set; }
    public int Remaining { get; set; }
    public bool IsOverQuota { get; set; }
}
