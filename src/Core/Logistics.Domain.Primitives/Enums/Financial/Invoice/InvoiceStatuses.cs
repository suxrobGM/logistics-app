namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// Status sets shared by every receivables figure, so the invoice dashboard, company stats and
/// financials report count the same invoices. Arrays rather than predicates so EF can translate
/// <c>Contains</c> to SQL.
/// </summary>
public static class InvoiceStatuses
{
    /// <summary>
    /// Billed and not yet paid in full. An open invoice past its due date is overdue whatever its
    /// stored status: <see cref="InvoiceStatus.Overdue"/> is only written when payments are recalculated.
    /// </summary>
    public static readonly InvoiceStatus[] Open =
    [
        InvoiceStatus.Issued, InvoiceStatus.Sent, InvoiceStatus.PartiallyPaid, InvoiceStatus.Overdue
    ];

    /// <summary>Open or paid: everything a customer was actually billed, excluding drafts and cancellations.</summary>
    public static readonly InvoiceStatus[] Receivable = [.. Open, InvoiceStatus.Paid];
}
