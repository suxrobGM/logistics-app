using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public record DriverHosStatusDto
{
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public DutyStatus CurrentDutyStatus { get; set; }
    public string? CurrentDutyStatusDisplay { get; set; }
    public DateTime StatusChangedAt { get; set; }

    public int DrivingMinutesRemaining { get; set; }
    public int OnDutyMinutesRemaining { get; set; }
    public int CycleMinutesRemaining { get; set; }

    public string? DrivingTimeRemainingDisplay { get; set; }
    public string? OnDutyTimeRemainingDisplay { get; set; }
    public string? CycleTimeRemainingDisplay { get; set; }

    public TimeSpan? TimeUntilBreakRequired { get; set; }
    public bool IsInViolation { get; set; }
    public bool IsAvailableForDispatch { get; set; }

    public DateTime LastUpdatedAt { get; set; }
    public DateTime? NextMandatoryBreakAt { get; set; }

    public EldProviderType ProviderType { get; set; }
}
