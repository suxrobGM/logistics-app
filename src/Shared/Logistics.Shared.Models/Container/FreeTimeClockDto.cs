using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public record FreeTimeClockDto
{
    public DateOnly? Deadline { get; init; }
    public FreeTimeStatus Status { get; init; }
    public int? DaysRemaining { get; init; }
    public int DaysOver { get; init; }
    public decimal? RatePerDay { get; init; }
    public decimal Charge { get; init; }
}
