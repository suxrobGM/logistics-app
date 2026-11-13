namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// Outcome of the most recent push of a local entity to the accounting provider.
/// </summary>
public enum QboSyncStatus
{
    Synced = 1,
    Failed = 2,

    /// <summary>The provider record was deactivated (customer) or voided (invoice, payment).</summary>
    Retired = 3
}
