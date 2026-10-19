namespace Logistics.Domain.Entities;

/// <summary>
///     An expense charged to one truck. Declares <see cref="TruckId"/> once for the TPH siblings, so
///     they share a single <c>truck_id</c> column, index and foreign key on <c>expenses</c>.
/// </summary>
public abstract class TruckLinkedExpense : Expense
{
    /// <summary>
    ///     The truck this expense is for.
    /// </summary>
    public required Guid TruckId { get; set; }

    /// <summary>
    ///     Navigation property for the truck.
    /// </summary>
    public virtual Truck Truck { get; set; } = null!;
}
