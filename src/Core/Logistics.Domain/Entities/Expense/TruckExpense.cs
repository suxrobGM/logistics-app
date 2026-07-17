using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

/// <summary>
///     Expenses related to a specific truck (fuel, maintenance, tires, registration, tolls, parking).
/// </summary>
public class TruckExpense : Expense
{
    public override ExpenseType Type { get; set; } = ExpenseType.Truck;

    /// <summary>
    ///     The truck this expense is for.
    /// </summary>
    public required Guid TruckId { get; set; }

    /// <summary>
    ///     Navigation property for the truck.
    /// </summary>
    public virtual Truck Truck { get; set; } = null!;

    /// <summary>
    ///     Category of the truck expense.
    /// </summary>
    public TruckExpenseCategory Category { get; set; }

    /// <summary>
    ///     Odometer reading at the time of expense (optional).
    /// </summary>
    public int? OdometerReading { get; set; }

    /// <summary>
    ///     Quantity purchased (typically fuel volume). Captured in <see cref="QuantityUnit"/>;
    ///     never auto-converted between units.
    /// </summary>
    public decimal? Quantity { get; set; }

    /// <summary>
    ///     Unit the <see cref="Quantity"/> is expressed in (Gallons / Liters).
    /// </summary>
    public VolumeUnit? QuantityUnit { get; set; }

    /// <summary>
    ///     Jurisdiction (country + state/province) where the purchase happened.
    ///     Set by fuel-card sync or manual entry; feeds IFTA per-jurisdiction gallons.
    /// </summary>
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
