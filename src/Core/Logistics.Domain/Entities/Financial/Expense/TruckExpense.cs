using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

/// <summary>
///     Expenses related to a specific truck (fuel, maintenance, tires, registration, tolls, parking).
/// </summary>
public class TruckExpense : TruckLinkedExpense
{
    public override ExpenseType Type { get; set; } = ExpenseType.Truck;

    /// <summary>
    ///     Category of the truck expense.
    /// </summary>
    public TruckExpenseCategory Category { get; set; }

    /// <summary>
    ///     Odometer reading at the time of expense, in meters (optional).
    /// </summary>
    public long? OdometerMeters { get; set; }

    /// <summary>Fuel volume purchased, in <see cref="QuantityUnit"/>; stored as entered and never converted between units.</summary>
    public decimal? Quantity { get; set; }

    /// <summary>
    ///     Unit the <see cref="Quantity"/> is expressed in (Gallons / Liters).
    /// </summary>
    public VolumeUnit? QuantityUnit { get; set; }

    /// <summary>Jurisdiction where the fuel was bought; IFTA reports sum fuel volume per jurisdiction from it.</summary>
    public TaxJurisdiction? PurchaseJurisdiction { get; set; }

    /// <summary>
    ///     Fuel card provider this expense was imported from, when auto-created by sync.
    /// </summary>
    public FuelCardProviderType? FuelCardProvider { get; set; }

    /// <summary>
    ///     Provider-side transaction ID for expenses imported from a fuel card (idempotency).
    /// </summary>
    public string? ExternalTransactionId { get; set; }

    /// <summary>
    ///     Unit price paid (per gallon/liter), when known.
    /// </summary>
    public decimal? PricePerUnit { get; set; }
}
