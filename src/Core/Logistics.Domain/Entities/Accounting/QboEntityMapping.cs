using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Sync-state record linking a local domain entity to its counterpart in the accounting
/// provider. Keeps external IDs and idempotency bookkeeping out of the domain entities.
/// </summary>
public class QboEntityMapping : AuditableEntity, ITenantEntity
{
    /// <summary>
    /// The local domain entity's Id (e.g. Customer.Id, LoadInvoice.Id, Payment.Id, Expense.Id).
    /// </summary>
    public required Guid LocalId { get; set; }

    /// <summary>
    /// Which kind of local entity <see cref="LocalId"/> refers to.
    /// </summary>
    public required QboLocalEntityType LocalEntityType { get; set; }

    /// <summary>
    /// The corresponding entity ID in the accounting provider (QBO object Id).
    /// </summary>
    public string? QboId { get; set; }

    /// <summary>
    /// QBO optimistic-concurrency token; must be sent back on updates.
    /// </summary>
    public string? QboSyncToken { get; set; }

    /// <summary>
    /// Content hash of the local entity at the time it was last pushed. Used to skip
    /// unchanged entities and keep pushes idempotent.
    /// </summary>
    public string? LastSyncedHash { get; set; }

    /// <summary>
    /// When this entity was last pushed successfully. Rows edited after this are pushed again.
    /// </summary>
    public DateTime? LastSyncedAt { get; set; }

    /// <summary>
    /// Outcome of the most recent push.
    /// </summary>
    public QboSyncStatus SyncStatus { get; set; } = QboSyncStatus.Synced;

    /// <summary>
    /// Error detail from the most recent failed push, if any.
    /// </summary>
    public string? LastError { get; set; }
}
