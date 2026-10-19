using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

public abstract partial class Invoice : AuditableEntity, ITenantEntity, IAuditableEntity
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

    /// <summary>
    /// Swaps the lines <paramref name="owns"/> matches for <paramref name="fresh"/>, which go first in
    /// the line order; the other lines keep their relative order after them. Returns the removed
    /// lines. The caller registers the removed and added lines with the repository.
    /// </summary>
    public IReadOnlyList<InvoiceLineItem> ReplaceLines(Func<InvoiceLineItem, bool> owns, IReadOnlyList<InvoiceLineItem> fresh)
    {
        var removed = LineItems.Where(owns).ToList();
        foreach (var line in removed)
        {
            LineItems.Remove(line);
        }

        var order = fresh.Count;
        foreach (var other in LineItems.OrderBy(li => li.Order))
        {
            other.Order = order++;
        }

        for (var i = 0; i < fresh.Count; i++)
        {
            fresh[i].Order = i;
        }

        LineItems.AddRange(fresh);
        return removed;
    }

    public void ApplyPayment(Payment payment)
    {
        Payments.Add(payment);
        RecalculatePaymentStatus(DateTime.UtcNow);
    }

    /// <summary>
    /// Reverses <see cref="ApplyPayment"/>: drops the payment and recalculates the status. The caller
    /// deletes the payment.
    /// </summary>
    public void RemovePayment(Payment payment, DateTime now)
    {
        Payments.Remove(payment);
        RecalculatePaymentStatus(now);
    }

    /// <summary>
    /// Sets the status from what the payments add up to. Call it after a payment's amount changes.
    /// </summary>
    public void RecalculatePaymentStatus(DateTime now)
    {
        var paid = Payments.Sum(p => p.Amount.Amount);
        if (paid > 0m)
        {
            Status = paid >= Total.Amount ? InvoiceStatus.Paid : InvoiceStatus.PartiallyPaid;
        }
        else
        {
            Status = UnpaidStatus(now);
        }
    }

    /// <summary>The status of an invoice with nothing paid: from the due date and whether it was sent.</summary>
    protected virtual InvoiceStatus UnpaidStatus(DateTime now)
    {
        if (DueDate < now)
        {
            return InvoiceStatus.Overdue;
        }

        return SentAt is not null ? InvoiceStatus.Sent : InvoiceStatus.Issued;
    }
}
