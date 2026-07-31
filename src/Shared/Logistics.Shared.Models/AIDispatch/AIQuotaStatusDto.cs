namespace Logistics.Shared.Models;

/// <summary>
/// Tenant-facing quota status: usage as a percentage, no raw budget numbers. The one dollar
/// figure exposed is <see cref="OverageChargesUsd"/>, so overage never surprises on the invoice.
/// </summary>
public record AIQuotaStatusDto
{
    public double UsagePercent { get; set; }
    public bool IsOverQuota { get; set; }
    public string? PlanName { get; set; }
    public DateTime? ResetsAt { get; set; }

    /// <summary>Billed (marked-up) overage accrued this week - what Stripe will invoice.</summary>
    public decimal OverageChargesUsd { get; set; }

    /// <summary>True when the budget is spent and the tenant opted to pause AI over billing overage.</summary>
    public bool OverageBlocked { get; set; }
}
