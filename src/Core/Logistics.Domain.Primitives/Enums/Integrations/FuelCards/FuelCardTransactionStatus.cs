namespace Logistics.Domain.Primitives.Enums;

public enum FuelCardTransactionStatus
{
    /// <summary>Not yet matched to a truck; sits in the review queue.</summary>
    Pending,

    /// <summary>Matched to a truck and materialized as a TruckExpense.</summary>
    Matched,

    /// <summary>Dismissed by a dispatcher (e.g. cash advance, non-fleet purchase).</summary>
    Ignored
}
