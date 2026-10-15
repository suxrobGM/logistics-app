using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

/// <summary>
/// Dwell and detention at one stop of a load. Billable hours and the charge are final once the
/// truck departs; while <see cref="IsOngoing"/> they are as of now.
/// </summary>
public record StopDetentionDto
{
    public Guid TripStopId { get; set; }
    public TripStopType StopType { get; set; }
    public DateTime ArrivedAt { get; set; }
    public DateTime? DepartedAt { get; set; }
    public int DwellMinutes { get; set; }
    public int FreeMinutes { get; set; }
    public int OverMinutes { get; set; }
    public decimal BillableHours { get; set; }
    public decimal? RatePerHour { get; set; }
    public decimal Charge { get; set; }
    public bool IsOngoing { get; set; }

    /// <summary>A detention line for this stop is on the load.</summary>
    public bool IsBilled { get; set; }
}
