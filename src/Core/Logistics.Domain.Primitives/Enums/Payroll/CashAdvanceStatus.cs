namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// A driver's request starts as <see cref="Requested"/>; one a dispatcher issues starts <see cref="Approved"/>.
/// An approved advance is deducted from the next settlement created for the driver.
/// </summary>
public enum CashAdvanceStatus
{
    Requested,
    Approved,
    Rejected
}
