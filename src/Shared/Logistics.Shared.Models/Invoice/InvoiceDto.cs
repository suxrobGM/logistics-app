using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Shared.Models;

public record InvoiceDto
{
    public Guid Id { get; set; }
  
    public long Number { get; set; }
    public InvoiceType Type { get; set; }
    public InvoiceStatus Status { get; set; }
    public DateTime CreatedDate { get; set; }
    
    /// <summary>
    /// Total inclusive of tax & discounts.
    /// </summary>
    public Money Total { get; set; } = null!;
    
    public string? Notes { get; set; }
    public DateTime? DueDate { get; set; }
    public string? StripeInvoiceId { get; set; }
    
    public IEnumerable<PaymentDto> Payments { get; set; } = [];
    
    // LoadInvoice fields
    public long LoadNumber { get; set; }
    public Guid? LoadId { get; set; }
    public Guid? CustomerId { get; set; }
    public CustomerDto? Customer { get; set; }
    
    // PayrollInvoice fields
    public Guid? EmployeeId { get; set; }
    public EmployeeDto? Employee { get; set; }
    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }
    
    // SubscriptionInvoice fields
    public Guid? SubscriptionId { get; set; }
    public DateTime? BillingPeriodStart { get; set; }
    public DateTime? BillingPeriodEnd { get; set; }
}
