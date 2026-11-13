namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// Type of time entry for hourly employees.
/// </summary>
public enum TimeEntryType
{
    Regular,
    Overtime,
    DoubleTime,
    PaidTimeOff,
    Holiday
}
