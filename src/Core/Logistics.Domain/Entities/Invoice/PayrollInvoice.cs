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
}
