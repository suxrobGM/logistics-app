using Logistics.Domain.Core;

namespace Logistics.Domain.Persistence;

/// <summary>
///     Generic Unit of Work for EF-backed aggregates. Parameterized by a marker interface
///     so derived UoWs (Tenant/Master) can constrain repositories to their domain.
/// </summary>
/// <typeparam name="TMarker">
///     Marker interface implemented by entities in this DbContext (e.g., ITenantEntity,
///     IMasterEntity).
/// </typeparam>
public interface IUnitOfWork<in TMarker> : IDisposable
{
    /// <summary>
    ///     Gets a cached repository instance for <typeparamref name="TEntity" /> with a Guid key.
    /// </summary>
    IRepository<TEntity, Guid> Repository<TEntity>()
        where TEntity : class, IEntity<Guid>, TMarker;

    /// <summary>
    ///     Gets a cached repository instance for <typeparamref name="TEntity" /> with key type <typeparamref name="TKey" />.
    /// </summary>
    IRepository<TEntity, TKey> Repository<TEntity, TKey>()
        where TEntity : class, IEntity<TKey>, TMarker;

    /// <summary>
    ///     Persists pending changes to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    /// <summary>
    ///     Runs one SQL command immediately, with every interpolated value sent as a parameter. For an
    ///     atomic statement EF cannot express, such as an upsert. It bypasses change tracking: tracked
    ///     entities are not refreshed and pending changes are not saved.
    /// </summary>
    Task<int> ExecuteSqlAsync(FormattableString sql, CancellationToken ct = default);

    /// <summary>
    ///     Runs one SQL statement immediately, parameterized like <see cref="ExecuteSqlAsync" />, and returns
    ///     its single column, which must be named <c>"Value"</c>. Sent as written, so it may be a statement
    ///     with <c>RETURNING</c>. Bypasses change tracking the same way.
    /// </summary>
    Task<List<T>> SqlQueryAsync<T>(FormattableString sql, CancellationToken ct = default);

    /// <summary>
    ///     Begins a database transaction (idempotent; does nothing if one is already active).
    /// </summary>
    Task BeginTransactionAsync(CancellationToken ct = default);

    /// <summary>
    ///     Commits the current transaction (idempotent; does nothing if none exists).
    /// </summary>
    Task CommitTransactionAsync(CancellationToken ct = default);

    /// <summary>
    ///     Rolls back the current transaction (idempotent; does nothing if none exists).
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
