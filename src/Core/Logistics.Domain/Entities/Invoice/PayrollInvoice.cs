using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

public class PayrollInvoice : Invoice
{
    public override InvoiceType Type { get; set; } = InvoiceType.Payroll;
    public required Guid EmployeeId { get; set; }
    public virtual Employee Employee { get; set; } = null!;

    /// <summary>
    /// Week, fortnight or month covered.
    /// </summary>
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }

    #region Calculated Earnings Data

    /// <summary>
    /// Total distance driven during the period (in kilometers).
    /// Used for RatePerDistance salary type calculations.
    /// </summary>
    public double TotalDistanceDriven { get; set; }

    /// <summary>
    /// Total hours worked during the period.
    /// Used for Hourly salary type calculations.
    /// </summary>
    public decimal TotalHoursWorked { get; set; }

    /// <summary>
    /// Time entries included in this payroll invoice (for hourly employees).
    /// </summary>
    public virtual List<TimeEntry> TimeEntries { get; set; } = [];

    #endregion

    #region Approval Workflow

    /// <summary>
    /// ID of the user who approved the payroll invoice.
    /// </summary>
    public Guid? ApprovedById { get; set; }

    /// <summary>
    /// User who approved the payroll invoice.
    /// </summary>
    public virtual User? ApprovedBy { get; set; }

    /// <summary>
    /// Date and time when the payroll invoice was approved.
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// Notes added during approval.
    /// </summary>
    public string? ApprovalNotes { get; set; }

    /// <summary>
    /// Reason for rejection if the payroll invoice was rejected.
    /// </summary>
    public string? RejectionReason { get; set; }

    #endregion
}
