using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

/// <summary>
/// One row inside <see cref="Invoice.TaxBreakdownJson"/>. Aggregates tax across line items
/// that share the same jurisdiction + rate, mirroring how Stripe Tax breaks down a calculation.
/// </summary>
public sealed record InvoiceTaxLine
{
    /// <summary>
    /// Effective tax rate as a percentage (e.g. 19.00 for 19%).
    /// </summary>
    public required decimal RatePercent { get; init; }

    /// <summary>
    /// Sum of net amounts subject to this rate, in the invoice's currency.
    /// </summary>
    public required decimal BaseAmount { get; init; }

    /// <summary>
    /// Tax owed on <see cref="BaseAmount"/> at <see cref="RatePercent"/>.
    /// Always 0 when the parent invoice is in reverse-charge mode.
    /// </summary>
    public required decimal TaxAmount { get; init; }

    public required TaxJurisdiction Jurisdiction { get; init; }

    /// <summary>
    /// Stripe Tax product code (txcd_*) or local tax-code identifier; null for fallback rates.
    /// </summary>
    public string? TaxCode { get; init; }

    /// <summary>
    /// Human-readable label (e.g. "VAT 19%", "California state sales tax", "Reverse charge").
    /// </summary>
    public string? Description { get; init; }
}
