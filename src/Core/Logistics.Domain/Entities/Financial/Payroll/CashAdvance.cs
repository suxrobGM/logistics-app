using Logistics.Domain.Core;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

/// <summary>Money advanced to a driver, deducted once from the next settlement created for that driver.</summary>
public class CashAdvance : AuditableEntity, ITenantEntity
{
    public Guid EmployeeId { get; set; }
    public virtual Employee Employee { get; set; } = null!;

    public required decimal Amount { get; set; }
    public string? Reason { get; set; }

    /// <summary>The settlement the advance was deducted from; null while it is still owed.</summary>
    public Guid? PayrollInvoiceId { get; set; }
}
