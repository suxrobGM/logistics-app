using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

/// <summary>
/// A fuel card transaction pulled from a provider. Every synced transaction lands here
/// (matched or not) — the (ProviderType, ExternalTransactionId) unique index is the sync
/// idempotency key, Pending rows form the dispatcher review queue, and RawPayloadJson
/// preserves the provider record for disputes.
/// </summary>
public class FuelCardTransaction : AuditableEntity, ITenantEntity
{
    public required FuelCardProviderType ProviderType { get; set; }

    /// <summary>
    /// Provider-side transaction identifier. Unique per provider; the sync idempotency key.
    /// </summary>
    public required string ExternalTransactionId { get; set; }

    /// <summary>
    /// When the purchase happened (UTC).
    /// </summary>
    public required DateTime TransactionDate { get; set; }

    public required Money Amount { get; set; }

    /// <summary>
    /// Fuel volume purchased, in <see cref="QuantityUnit"/>.
    /// </summary>
    public decimal? Quantity { get; set; }

    public VolumeUnit? QuantityUnit { get; set; }

    public decimal? PricePerUnit { get; set; }

    /// <summary>
    /// Provider product category (diesel, DEF, reefer fuel, cash advance, ...).
    /// </summary>
    public string? ProductCategory { get; set; }

    public string? MerchantName { get; set; }
    public string? MerchantCity { get; set; }

    /// <summary>
    /// Jurisdiction (country + state/province) where the fuel was purchased. Feeds IFTA.
    /// </summary>
    public TaxJurisdiction? PurchaseJurisdiction { get; set; }

    public string? CardNumberMasked { get; set; }

    /// <summary>
    /// Provider-side card identifier, used to remember card → truck mappings.
    /// </summary>
    public string? ExternalCardId { get; set; }

    /// <summary>
    /// Unit/vehicle number the provider attached to the transaction; used for truck auto-matching.
    /// </summary>
    public string? UnitNumber { get; set; }

    public string? DriverName { get; set; }

    public FuelCardTransactionStatus Status { get; set; } = FuelCardTransactionStatus.Pending;

    /// <summary>
    /// The truck this transaction was matched to.
    /// </summary>
    public Guid? TruckId { get; set; }
    public virtual Truck? Truck { get; set; }

    /// <summary>
    /// The TruckExpense materialized from this transaction when matched.
    /// </summary>
    public Guid? ExpenseId { get; set; }

    /// <summary>
    /// Raw provider payload for audits and disputes.
    /// </summary>
    public string? RawPayloadJson { get; set; }
}
