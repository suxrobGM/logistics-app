using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Shared.Models;

/// <summary>
/// Invoice data exposed via public payment links (limited fields for security).
/// </summary>
public record PublicInvoiceDto
{
    public Guid Id { get; set; }
    public long Number { get; set; }
    public InvoiceStatus Status { get; set; }
    public Money Total { get; set; } = null!;
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Amount remaining to be paid (Total - sum of payments).
    /// </summary>
    public decimal AmountDue { get; set; }

    /// <summary>
    /// Line items on the invoice.
    /// </summary>
    public IEnumerable<InvoiceLineItemDto> LineItems { get; set; } = [];

    /// <summary>
    /// Company name of the trucking company (tenant).
    /// </summary>
    public string? CompanyName { get; set; }

    /// <summary>
    /// Customer name associated with the invoice.
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// Load number for reference.
    /// </summary>
    public long? LoadNumber { get; set; }

    /// <summary>
    /// Whether the invoice can accept payments (not fully paid or cancelled).
    /// </summary>
    public bool CanPay => Status != InvoiceStatus.Paid && Status != InvoiceStatus.Cancelled;
}
