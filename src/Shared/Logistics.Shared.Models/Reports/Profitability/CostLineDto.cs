namespace Logistics.Shared.Models;

public record CostLineDto
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
