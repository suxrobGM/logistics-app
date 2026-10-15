namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// Where a demurrage or per-diem clock stands.
/// </summary>
public enum FreeTimeStatus
{
    /// <summary>No deadline entered.</summary>
    NotSet,

    /// <summary>Running, more than one day of free time left.</summary>
    Running,

    /// <summary>Running, the deadline is today or tomorrow.</summary>
    DueSoon,

    /// <summary>Running, past the deadline - charges are accruing.</summary>
    Overdue,

    /// <summary>The clock stopped (picked up / returned / street-turned); any charge is final.</summary>
    Stopped
}
