using System.Globalization;
using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

public abstract partial class Invoice : AuditableEntity, ITenantEntity, IAuditableEntity
{
    public long Number { get; set; }
    public abstract InvoiceType Type { get; set; }
    public InvoiceStatus Status { get; set; }

    /// <summary>Pre-tax sum of the line nets; set by <see cref="RecalculateTotals"/>.</summary>
    public required decimal Subtotal { get; set; }

    /// <summary>Always 0 under <see cref="TaxBehavior.ReverseCharge"/>.</summary>
    public required decimal TaxTotal { get; set; }

    /// <summary>Includes tax and discounts; Subtotal + TaxTotal under exclusive pricing.</summary>
    public required decimal Total { get; set; }

    public TaxBehavior TaxBehavior { get; set; } = TaxBehavior.Exclusive;

    /// <summary>Read and write it through <see cref="GetTaxBreakdown"/> and <see cref="SetTaxBreakdown"/>.</summary>
    public string? TaxBreakdownJson { get; set; }

    public string? Notes { get; set; }
    public DateTime? DueDate { get; set; }

    public virtual List<Payment> Payments { get; set; } = [];
    public virtual List<InvoiceLineItem> LineItems { get; set; } = [];
    public virtual List<PaymentLink> PaymentLinks { get; set; } = [];

    public DateTime? SentAt { get; set; }

    public string? SentToEmail { get; set; }

    /// <summary>
    /// Puts <paramref name="fresh"/> first in line order and returns the removed lines. The caller registers
    /// both with the repository.
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

    public decimal AmountPaid => SumPaid(Payments);

    public decimal AmountDue => Total - AmountPaid;

    /// <summary>Why the invoice's lines can't change - it is paid or cancelled - or null when they can.</summary>
    public string? GetEditBlockedReason() => Status switch
    {
        InvoiceStatus.Paid => "Cannot modify a paid invoice.",
        InvoiceStatus.Cancelled => "Cannot modify a cancelled invoice.",
        _ => null
    };

    /// <summary>Why a payment of <paramref name="amount" /> can't be taken against this invoice, or null when it can.</summary>
    public string? GetPaymentBlockedReason(decimal amount)
    {
        if (Status == InvoiceStatus.Paid)
        {
            return "This invoice has already been paid.";
        }

        if (Status == InvoiceStatus.Cancelled)
        {
            return "This invoice has been cancelled.";
        }

        if (amount <= 0)
        {
            return "Payment amount must be greater than zero.";
        }

        return amount > AmountDue
            ? string.Create(CultureInfo.InvariantCulture, $"Payment amount cannot exceed amount due ({AmountDue:N2}).")
            : null;
    }

    /// <summary>On a payroll the amount is signed by <paramref name="type" />. The caller adds the line to its repository.</summary>
    public InvoiceLineItem AddLineItem(
        string description, InvoiceLineItemType type, decimal amount, int quantity, string? notes)
    {
        var lineItem = new InvoiceLineItem
        {
            InvoiceId = Id,
            Description = description,
            Type = type,
            Amount = SignLineAmount(type, amount),
            Quantity = quantity,
            Order = LineItems.Count > 0 ? LineItems.Max(li => li.Order) + 1 : 0,
            Notes = notes
        };

        LineItems.Add(lineItem);
        return lineItem;
    }

    protected virtual decimal SignLineAmount(InvoiceLineItemType type, decimal amount) => amount;

    /// <summary>Counts only <see cref="PaymentStatus.Paid"/> payments.</summary>
    public static decimal SumPaid(IEnumerable<Payment> payments) => payments
        .Where(p => p.Status == PaymentStatus.Paid)
        .Sum(p => p.Amount);

    /// <summary>Links the payment whatever its status; a pending one pays only once it settles and the status is recalculated.</summary>
    public void ApplyPayment(Payment payment)
    {
        payment.InvoiceId = Id;
        payment.Invoice = this;
        Payments.Add(payment);
        RecalculatePaymentStatus(DateTime.UtcNow);
    }

    /// <summary>The caller deletes the payment.</summary>
    public void RemovePayment(Payment payment, DateTime now)
    {
        Payments.Remove(payment);
        RecalculatePaymentStatus(now);
    }

    /// <summary>Call after a payment's amount or status changes.</summary>
    public void RecalculatePaymentStatus(DateTime now)
    {
        var paid = AmountPaid;
        if (paid > 0m)
        {
            Status = paid >= Total ? InvoiceStatus.Paid : InvoiceStatus.PartiallyPaid;
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
