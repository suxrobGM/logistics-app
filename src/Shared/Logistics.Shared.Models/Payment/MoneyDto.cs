namespace Logistics.Shared.Models;

public record MoneyDto
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
}