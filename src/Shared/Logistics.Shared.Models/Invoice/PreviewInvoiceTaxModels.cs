using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Shared.Models;

public record PreviewInvoiceTaxRequest
{
    public required Guid CustomerId { get; set; }
    public required string Currency { get; set; }
    public required IReadOnlyList<PreviewInvoiceTaxLineItem> LineItems { get; set; }
}

public record PreviewInvoiceTaxLineItem
{
    public Guid LineItemId { get; set; } = Guid.NewGuid();
    public required string Description { get; set; }
    public required InvoiceLineItemType Type { get; set; }
    public required decimal Amount { get; set; }
    public int Quantity { get; set; } = 1;
    public string? TaxCode { get; set; }
}

public record PreviewInvoiceTaxResponse
{
    public TaxBehavior TaxBehavior { get; set; }
    public Money Subtotal { get; set; } = null!;
    public Money TaxTotal { get; set; } = null!;
    public Money Total { get; set; } = null!;
    public IEnumerable<InvoiceTaxLineDto> Breakdown { get; set; } = [];
    public IEnumerable<PreviewLineItemTax> Lines { get; set; } = [];
    public string? Warning { get; set; }
}

public record PreviewLineItemTax
{
    public Guid LineItemId { get; set; }
    public decimal RatePercent { get; set; }
    public decimal TaxAmount { get; set; }
    public string? TaxCode { get; set; }
}
