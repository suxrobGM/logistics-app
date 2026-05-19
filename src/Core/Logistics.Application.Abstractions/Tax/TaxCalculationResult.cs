using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Abstractions.Tax;

/// <summary>
/// Output of a tax calculation. Per-line entries map back to <see cref="InvoiceLineItem.Id"/>;
/// <see cref="Breakdown"/> aggregates lines that share rate + jurisdiction (matches the layout
/// of Stripe's <c>tax_breakdown</c>).
/// </summary>
public sealed record TaxCalculationResult
{
    public required TaxBehavior TaxBehavior { get; init; }
    public required IReadOnlyList<TaxCalculationLineResult> Lines { get; init; }
    public required IReadOnlyList<InvoiceTaxLine> Breakdown { get; init; }

    /// <summary>
    /// Optional warning surfaced when Stripe returned <c>taxability_reason = not_collecting</c>
    /// for one or more jurisdictions — the tenant isn't registered there.
    /// </summary>
    public string? Warning { get; init; }

    public static TaxCalculationResult Empty(TaxBehavior behavior = TaxBehavior.Exclusive) => new()
    {
        TaxBehavior = behavior,
        Lines = [],
        Breakdown = []
    };
}

public sealed record TaxCalculationLineResult
{
    public required Guid LineItemId { get; init; }
    public required decimal RatePercent { get; init; }
    public required decimal TaxAmount { get; init; }
    public string? TaxCode { get; init; }
}
