using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

public class DriverHosStatus : Entity, ITenantEntity
{
    public Guid EmployeeId { get; set; }
    public virtual Employee Employee { get; set; } = null!;

    /// <summary>
    /// The driver's ID in the ELD provider's system
    /// </summary>
    public string? ExternalDriverId { get; set; }

    public EldProviderType ProviderType { get; set; }

    /// <summary>
    /// Current duty status of the driver
    /// </summary>
    public required DutyStatus CurrentDutyStatus { get; set; }

    /// <summary>
    /// When the driver last changed duty status
    /// </summary>
    public DateTime StatusChangedAt { get; set; }

    /// <summary>
    /// Remaining driving time in minutes (11-hour rule)
    /// </summary>
    public int DrivingMinutesRemaining { get; set; }

    /// <summary>
    /// Remaining on-duty time in minutes (14-hour rule)
    /// </summary>
    public int OnDutyMinutesRemaining { get; set; }

    /// <summary>
    /// Remaining cycle time in minutes (70-hour/8-day rule)
    /// </summary>
    public int CycleMinutesRemaining { get; set; }

    /// <summary>
    /// Time until a 30-minute break is required
    /// </summary>
    public TimeSpan? TimeUntilBreakRequired { get; set; }

    /// <summary>
    /// Whether the driver is currently in an HOS violation
    /// </summary>
    public bool IsInViolation { get; set; }

    /// <summary>
    /// When this record was last updated from the ELD provider
    /// </summary>
    public DateTime LastUpdatedAt { get; set; }

    /// <summary>
    /// When the driver must take their next mandatory break
    /// </summary>
    public DateTime? NextMandatoryBreakAt { get; set; }

    /// <summary>
    /// Checks if the driver is available for dispatch based on remaining hours
    /// </summary>
    public bool IsAvailableForDispatch(int minimumDrivingMinutes = 60)
    {
        return !IsInViolation && DrivingMinutesRemaining >= minimumDrivingMinutes;
    }

    /// <summary>
    /// Gets a formatted string of remaining driving time
    /// </summary>
    public string GetDrivingTimeRemainingDisplay()
    {
        var hours = DrivingMinutesRemaining / 60;
        var minutes = DrivingMinutesRemaining % 60;
        return $"{hours}h {minutes}m";
    }

    /// <summary>
    /// Gets a formatted string of remaining on-duty time
    /// </summary>
    public string GetOnDutyTimeRemainingDisplay()
    {
        var hours = OnDutyMinutesRemaining / 60;
        var minutes = OnDutyMinutesRemaining % 60;
        return $"{hours}h {minutes}m";
    }
}
