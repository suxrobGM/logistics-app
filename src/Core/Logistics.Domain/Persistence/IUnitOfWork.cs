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

    /// <summary>
    ///     Reverts tracked changes in the current DbContext without touching the database.
    ///     Added entities are detached; modified/deleted entities revert to Unchanged.
    /// </summary>
    //void RollbackTrackedChanges();

    /// <summary>
    ///     Executes a raw SQL command (non-query).
    /// </summary>
    Task ExecuteRawSql(string sql, CancellationToken ct = default);

    /// <summary>
    ///     Executes a raw SQL query returning a list of <typeparamref name="TSqlResponse" />.
    /// </summary>
    Task<List<TSqlResponse>> ExecuteRawSql<TSqlResponse>(string sql, CancellationToken ct = default)
        where TSqlResponse : class;

    /// <summary>
    ///     Executes an interpolated raw SQL query returning a list of <typeparamref name="TSqlResponse" />.
    /// </summary>
    Task<List<TSqlResponse>> ExecuteRawSql<TSqlResponse>(FormattableString sql, CancellationToken ct = default)
        where TSqlResponse : class;
}
