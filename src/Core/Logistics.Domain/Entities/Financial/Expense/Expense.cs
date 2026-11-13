using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

/// <summary>
///     Base class for all expense types using Table Per Hierarchy (TPH).
/// </summary>
public abstract class Expense : AuditableEntity, ITenantEntity
{
    public long Number { get; set; }

    public abstract ExpenseType Type { get; set; }
    public ExpenseStatus Status { get; set; } = ExpenseStatus.Pending;

    /// <summary>
    ///     Total expense amount.
    /// </summary>
    public required decimal Amount { get; set; }

    /// <summary>
    ///     Vendor or payee name.
    /// </summary>
    public string? VendorName { get; set; }

    /// <summary>
    ///     Date when the expense occurred.
    /// </summary>
    public required DateOnly ExpenseDate { get; set; }

    /// <summary>
    ///     Path to the receipt blob in storage. Required for all expenses.
    /// </summary>
    public string? ReceiptBlobPath { get; set; }

    /// <summary>
    ///     Notes or description of the expense.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    ///     User ID of the approver.
    /// </summary>
    public string? ApprovedById { get; set; }

    /// <summary>
    ///     Date when the expense was approved or rejected.
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    ///     Reason for rejection (if rejected).
    /// </summary>
    public string? RejectionReason { get; set; }

    public void Approve(string approverId)
    {
        Status = ExpenseStatus.Approved;
        ApprovedById = approverId;
        ApprovedAt = DateTime.UtcNow;
        RejectionReason = null;
    }

    public void Reject(string approverId, string reason)
    {
        Status = ExpenseStatus.Rejected;
        ApprovedById = approverId;
        ApprovedAt = DateTime.UtcNow;
        RejectionReason = reason;
    }
}
