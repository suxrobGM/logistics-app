using Logistics.Domain.Core;
using Logistics.Shared.Enums;

namespace Logistics.Domain.Entities;

public class BillingInfo : Entity, ITenantEntity
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime PaymentDate { get; set; }
    public BillingType BillingType { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Payment { get; set; }
    public bool IsPaid { get; set; }
    public string? Note { get; set; }
    
    public string? EmployeeId { get; set; }
    public virtual Employee? Employee { get; set; }
}
