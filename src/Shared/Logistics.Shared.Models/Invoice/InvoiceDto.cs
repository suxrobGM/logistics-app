using Logistics.Shared.Consts;

namespace Logistics.Shared.Models;

public record InvoiceDto
{
    public string Id { get; set; } = null!;
  
    public long Number { get; set; }
    public InvoiceType Type { get; set; }
    public InvoiceStatus Status { get; set; }
    
    /// <summary>
    /// Total inclusive of tax & discounts.
    /// </summary>
    public MoneyDto Total { get; set; } = null!;
    
    public string? Notes { get; set; }
    public DateTime? DueDate { get; set; }
    public string? StripeInvoiceId { get; set; }
    
    public IEnumerable<PaymentDto> Payments { get; set; } = [];
    
    // LoadInvoice fields
    public long LoadNumber { get; set; }
    public string? LoadId { get; set; }
    public string? CustomerId { get; set; }
    
    // PayrollInvoice fields
    public string? EmployeeId { get; set; }
    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }
    
    // SubscriptionInvoice fields
    public string? SubscriptionId { get; set; }
    public DateTime? BillingPeriodStart { get; set; }
    public DateTime? BillingPeriodEnd { get; set; }
}
