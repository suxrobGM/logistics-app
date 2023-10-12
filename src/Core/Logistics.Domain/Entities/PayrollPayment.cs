using Logistics.Domain.Core;

namespace Logistics.Domain.Entities;

public class PayrollPayment : Entity, ITenantEntity
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public string PaymentId { get; set; } = default!;
    public virtual Payment Payment { get; set; } = default!;

    public string? EmployeeId { get; set; }
    public virtual Employee? Employee { get; set; }
}
