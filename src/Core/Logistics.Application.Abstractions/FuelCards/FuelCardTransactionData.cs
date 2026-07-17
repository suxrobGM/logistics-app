using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Application.Abstractions.FuelCards;

/// <summary>
/// A fuel card transaction as pulled from a provider, before truck matching and persistence.
/// </summary>
public record FuelCardTransactionData
{
    public required string ExternalTransactionId { get; init; }

    /// <summary>Purchase timestamp in UTC.</summary>
    public required DateTime TransactionDate { get; init; }

    public required decimal Amount { get; init; }

    public string Currency { get; init; } = "USD";

    public decimal? Quantity { get; init; }

    public VolumeUnit? QuantityUnit { get; init; }

    public decimal? PricePerUnit { get; init; }

    public string? ProductCategory { get; init; }

    public string? MerchantName { get; init; }

    public string? MerchantCity { get; init; }

    public TaxJurisdiction? PurchaseJurisdiction { get; init; }

    public string? CardNumberMasked { get; init; }

    /// <summary>Provider-side card identifier, when the provider exposes one.</summary>
    public string? ExternalCardId { get; init; }

    /// <summary>Unit/vehicle number attached to the transaction; used for truck auto-matching.</summary>
    public string? UnitNumber { get; init; }

    public string? DriverName { get; init; }

    /// <summary>Raw provider payload for audits and disputes.</summary>
    public string? RawJson { get; init; }
}
