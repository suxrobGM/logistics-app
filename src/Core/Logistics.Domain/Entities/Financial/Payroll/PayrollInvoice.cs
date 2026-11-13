using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

public partial class PayrollInvoice : Invoice
{
    public override InvoiceType Type { get; set; } = InvoiceType.Payroll;

    /// <summary>A payroll invoice is paid only once approved, so losing its payments returns it there.</summary>
    protected override InvoiceStatus UnpaidStatus(DateTime now) => InvoiceStatus.Approved;

    public required Guid EmployeeId { get; set; }
    public virtual Employee Employee { get; set; } = null!;

    /// <summary>
    /// Week, fortnight or month covered.
    /// </summary>
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }

    /// <summary>The period's days as a UTC [start, day after end) range, for comparing timestamps.</summary>
    public (DateTime From, DateTime To) PeriodUtcRange() =>
        (PeriodStart.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            PeriodEnd.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));

    /// <summary>
    /// Weeks a weekly salary pays for: the period's days, both dates included, divided by 7 and rounded
    /// up, so a partial week pays as a full one whatever weekday the period starts on.
    /// </summary>
    public int PeriodWeeks()
    {
        var days = PeriodEnd.DayNumber - PeriodStart.DayNumber + 1;
        return (int)Math.Ceiling(days / 7.0);
    }

    /// <summary>
    /// Months a monthly salary pays for, counting the start month, so a partial month pays as a full
    /// one. A last month that ends before the start day of the month is not counted: the month before
    /// it already reached that day.
    /// </summary>
    public int PeriodMonths()
    {
        var months = (PeriodEnd.Year - PeriodStart.Year) * 12 + PeriodEnd.Month - PeriodStart.Month + 1;
        return PeriodEnd.Day < PeriodStart.Day ? months - 1 : months;
    }

    #region Calculated Earnings Data

    /// <summary>
    /// Total distance driven during the period, in meters.
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
