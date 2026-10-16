using System.Linq.Expressions;
using Logistics.Domain.Core;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Accounting.Services;

/// <summary>
/// The sync state of each pushed record (<see cref="QboEntityMapping"/>): which records are not up to date,
/// and the outcome of each push. A failed push is stored and does not abort the run; only a rejected token does.
/// </summary>
internal interface IAccountingMappingService : IApplicationService
{
    IQueryable<QboEntityMapping> Mappings { get; }

    /// <summary>
    /// Rows with no mapping, a failed push, or a push older than their last edit. Everything else is
    /// skipped without being loaded.
    /// </summary>
    Expression<Func<T, bool>> NotUpToDate<T>(QboLocalEntityType type) where T : AuditableEntity;

    /// <summary>Loads the mappings of these records into the run, skipping those already loaded.</summary>
    Task LoadAsync(SyncRun run, QboLocalEntityType type, IEnumerable<Guid> localIds, CancellationToken ct);

    /// <summary>Pushes one record if new or changed since its last push (by <paramref name="hash"/>).</summary>
    Task PushAsync(SyncRun run, QboLocalEntityType type, Guid localId, string hash,
        Func<QboWriteTarget, Task<QboUpsertResult>> write, CancellationToken ct);

    /// <summary>Deactivates or voids a pushed record in the provider; a record never pushed is left alone.</summary>
    Task RetireAsync(SyncRun run, QboLocalEntityType type, Guid localId,
        Func<QboWriteTarget, Task<QboUpsertResult>> retire, CancellationToken ct);

    /// <summary>
    /// Retires the pushed records of a hard-deleted type (customers, payments) whose row is gone.
    /// </summary>
    Task RetireRemovedAsync<T>(SyncRun run, QboLocalEntityType type,
        Func<QboWriteTarget, Task<QboUpsertResult>> retire, CancellationToken ct)
        where T : class, IEntity<Guid>, ITenantEntity;

    /// <summary>A record that cannot be pushed yet, shown to the user like any other sync error.</summary>
    Task RecordFailureAsync(SyncRun run, QboLocalEntityType type, Guid localId, string message, CancellationToken ct);
}
