using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

/// <summary>
/// Money paid to a driver ahead of settlement (a driver requests it, or a dispatcher issues it).
/// An approved advance is deducted, once, from the next settlement created for the driver.
/// </summary>
public class CashAdvance : AuditableEntity, ITenantEntity
{
    public Guid EmployeeId { get; set; }
    public virtual Employee Employee { get; set; } = null!;

    public required Money Amount { get; set; }
    public string? Reason { get; set; }

    public CashAdvanceStatus Status { get; set; } = CashAdvanceStatus.Requested;

    public Guid? ReviewedById { get; set; }
    public DateTime? ReviewedAt { get; set; }

    /// <summary>The settlement the advance was deducted from; null while it is still owed.</summary>
    public Guid? PayrollInvoiceId { get; set; }

    public void Approve(Guid reviewerId)
    {
        Status = CashAdvanceStatus.Approved;
        ReviewedById = reviewerId;
        ReviewedAt = DateTime.UtcNow;
    }

    public void Reject(Guid reviewerId)
    {
        Status = CashAdvanceStatus.Rejected;
        ReviewedById = reviewerId;
        ReviewedAt = DateTime.UtcNow;
    }
}
