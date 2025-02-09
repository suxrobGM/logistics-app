using Logistics.Domain.Core;

namespace Logistics.Domain.Entities;

public class Payroll : Entity, ITenantEntity
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public string PaymentId { get; set; } = null!;
    public virtual Payment Payment { get; set; } = null!;

    public string EmployeeId { get; set; } = null!;
    public virtual Employee Employee { get; set; } = null!;
}
