using Logistics.Domain.Primitives.Enums;

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
}
