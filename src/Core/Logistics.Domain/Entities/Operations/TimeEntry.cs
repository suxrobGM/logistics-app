using Logistics.Domain.Core;
using Logistics.Domain.Primitives;
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
    /// When work started, UTC.
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// When work ended, UTC; an overnight shift ends the next day.
    /// </summary>
    public DateTime EndedAt { get; set; }

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
        var duration = EndedAt - StartedAt;
        TotalHours = duration.TotalHours.ToDecimal();
    }
}
