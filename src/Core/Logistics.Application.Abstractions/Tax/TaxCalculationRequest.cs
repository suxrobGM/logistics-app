using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Application.Abstractions.Tax;

/// <summary>
/// Inputs to a tax calculation. The calculator decides reverse-charge / inclusive / exclusive
/// behavior based on tenant region, customer location, and (for EU) the customer's VAT ID.
/// </summary>
public sealed record TaxCalculationRequest
{
    /// <summary>Currency for all line amounts (3-letter ISO).</summary>
    public required string Currency { get; init; }

    /// <summary>Tenant id, used by <c>ManualTaxCalculator</c> to look up <c>TenantTaxRate</c> rows.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Operating region of the tenant — drives reverse-charge eligibility.</summary>
    public required Region TenantRegion { get; init; }

    /// <summary>Tenant billing address (used as origin for jurisdiction routing).</summary>
    public required Address TenantAddress { get; init; }

    /// <summary>Tenant VAT/EIN/etc. — required by Stripe Tax for EU outbound invoices.</summary>
    public string? TenantTaxId { get; init; }

    /// <summary>ISO-2 country where the tenant is tax-resident; falls back to <c>TenantAddress.Country</c>.</summary>
    public string? TenantTaxResidencyCountry { get; init; }

    /// <summary>Customer billing address (drives destination-based VAT/sales tax).</summary>
    public required Address CustomerAddress { get; init; }

    /// <summary>Customer VAT ID — presence on EU cross-border B2B triggers reverse charge.</summary>
    public string? CustomerTaxId { get; init; }

    /// <summary>Skip tax for charity/government/exempt customers.</summary>
    public bool IsCustomerVatExempt { get; init; }

    /// <summary>Line items as a flat list. Stable LineItemId lets the result be merged back per-line.</summary>
    public required IReadOnlyList<TaxCalculationLineItem> LineItems { get; init; }
}

/// <summary>
/// One line item input. <c>NetAmount</c> is the pre-tax amount (Quantity already applied).
/// </summary>
public sealed record TaxCalculationLineItem
{
    public required Guid LineItemId { get; init; }
    public required decimal NetAmount { get; init; }
    public string? TaxCode { get; init; }
    public string? Description { get; init; }
}
