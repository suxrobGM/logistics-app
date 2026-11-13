using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
///     Body shop repair expenses for trucks.
/// </summary>
public class BodyShopExpense : TruckLinkedExpense
{
    public override ExpenseType Type { get; set; } = ExpenseType.BodyShop;

    /// <summary>
    ///     Body shop vendor address.
    /// </summary>
    public string? VendorAddress { get; set; }

    /// <summary>
    ///     Body shop vendor phone number.
    /// </summary>
    public string? VendorPhone { get; set; }

    /// <summary>
    ///     Description of the repair work.
    /// </summary>
    public string? RepairDescription { get; set; }

    /// <summary>
    ///     Estimated completion date for the repair.
    /// </summary>
    public DateTime? EstimatedCompletionDate { get; set; }

    /// <summary>
    ///     Actual completion date of the repair.
    /// </summary>
    public DateTime? ActualCompletionDate { get; set; }
}
