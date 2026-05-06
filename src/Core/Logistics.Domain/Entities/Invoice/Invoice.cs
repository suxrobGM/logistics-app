using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

public abstract partial class Invoice : AuditableEntity, IMasterEntity, ITenantEntity, IAuditableEntity
{
    public long Number { get; set; }
    public abstract InvoiceType Type { get; set; }
    public InvoiceStatus Status { get; set; }

    /// <summary>
    /// Sum of line item nets (pre-tax) in the invoice currency. Set by <see cref="RecalculateTotals"/>.
    /// </summary>
    public required Money Subtotal { get; set; }

    /// <summary>
    /// Tax owed across all line items in the invoice currency. Always 0 when
    /// <see cref="TaxBehavior"/> is <see cref="TaxBehavior.ReverseCharge"/>.
    /// </summary>
    public required Money TaxTotal { get; set; }

    /// <summary>
    /// Total inclusive of tax &amp; discounts. Equals Subtotal + TaxTotal for exclusive pricing.
    /// </summary>
    public required Money Total { get; set; }

    /// <summary>
    /// How tax is applied to line items. Defaults to Exclusive (tax added on top).
    /// </summary>
    public TaxBehavior TaxBehavior { get; set; } = TaxBehavior.Exclusive;

    /// <summary>
    /// Persisted JSON of <see cref="InvoiceTaxLine"/> entries. Use
    /// <see cref="GetTaxBreakdown"/> / <see cref="SetTaxBreakdown"/> to access.
    /// </summary>
    public string? TaxBreakdownJson { get; set; }

    public string? Notes { get; set; }
    public DateTime? DueDate { get; set; }
    public string? StripeInvoiceId { get; set; }

    public virtual List<Payment> Payments { get; set; } = [];
    public virtual List<InvoiceLineItem> LineItems { get; set; } = [];
    public virtual List<PaymentLink> PaymentLinks { get; set; } = [];

    /// <summary>
    /// When the invoice was sent to the customer.
    /// </summary>
    public DateTime? SentAt { get; set; }

    /// <summary>
    /// Email address the invoice was sent to.
    /// </summary>
    public string? SentToEmail { get; set; }

    public void ApplyPayment(Payment payment)
    {
        Payments.Add(payment);
        var paid = Payments.Sum(p => p.Amount);
        Status = paid >= Total.Amount ? InvoiceStatus.Paid : InvoiceStatus.PartiallyPaid;
    }
}
