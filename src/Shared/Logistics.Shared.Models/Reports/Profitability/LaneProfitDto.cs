namespace Logistics.Shared.Models;

/// <summary>Loads grouped by origin and destination state. Per-distance figures are in the tenant's distance unit.</summary>
public record LaneProfitDto
{
    public string Lane { get; set; } = string.Empty;
    public int LoadCount { get; set; }
    public decimal Revenue { get; set; }
    public decimal Margin { get; set; }
    public decimal? RevenuePerDistance { get; set; }
    public decimal? MarginPerDistance { get; set; }
    public string? BestCustomer { get; set; }
    public string? WorstCustomer { get; set; }
}
