using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Represents a time entry for tracking hours worked by hourly employees.
/// </summary>
public class TimeEntry : AuditableEntity, ITenantEntity
{
    public required Guid EmployeeId { get; set; }
    public virtual Employee Employee { get; set; } = null!;

    /// <summary>
    /// The date of the time entry.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Start time of work.
    /// </summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// End time of work.
    /// </summary>
    public TimeSpan EndTime { get; set; }

    /// <summary>
    /// Total hours worked (can be manually adjusted).
    /// </summary>
    public decimal TotalHours { get; set; }

    /// <summary>
    /// Type of time entry (regular, overtime, PTO, etc.).
    /// </summary>
    public TimeEntryType Type { get; set; } = TimeEntryType.Regular;

    /// <summary>
    /// Optional link to the payroll invoice this entry was included in.
    /// </summary>
    public Guid? PayrollInvoiceId { get; set; }
    public virtual PayrollInvoice? PayrollInvoice { get; set; }

    /// <summary>
    /// Optional notes about the time entry.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Calculates total hours from start and end times.
    /// </summary>
    public void CalculateTotalHours()
    {
        var duration = EndTime - StartTime;
        TotalHours = (decimal)duration.TotalHours;
    }
}
