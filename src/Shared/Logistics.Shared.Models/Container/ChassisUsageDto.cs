namespace Logistics.Shared.Models;

public record ChassisUsageDto
{
    public int Days { get; init; }
    public decimal? RatePerDay { get; init; }
    public decimal Charge { get; init; }
}
