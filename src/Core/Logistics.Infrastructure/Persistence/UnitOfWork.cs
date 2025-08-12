using System.Collections.Concurrent;
using Logistics.Domain.Core;
using Logistics.Domain.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Logistics.Infrastructure.Persistence;

internal abstract class UnitOfWork<TMarker>(DbContext db) : IUnitOfWork<TMarker>
{
    private readonly ConcurrentDictionary<(Type entity, Type key), object> _repoCache = new();
    private IDbContextTransaction? _tx;

    protected DbContext Db { get; } = db;

    public IRepository<TEntity, Guid> Repository<TEntity>()
        where TEntity : class, IEntity<Guid>, TMarker
    {
        return Repository<TEntity, Guid>();
    }

    public IRepository<TEntity, TKey> Repository<TEntity, TKey>()
        where TEntity : class, IEntity<TKey>, TMarker
    {
        var key = (typeof(TEntity), typeof(TKey));
        return (IRepository<TEntity, TKey>)_repoCache.GetOrAdd(
            key,
            _ => CreateRepository<TEntity, TKey>());
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return Db.SaveChangesAsync(ct);
    }

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        _tx ??= await Db.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_tx is null)
        {
            return;
        }

        try
        {
            await Db.SaveChangesAsync(ct); // safe if already saved; keeps atomicity
            await _tx.CommitAsync(ct);
        }
        finally
        {
            await _tx.DisposeAsync();
            _tx = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_tx is null)
        {
            return;
        }

        try
        {
            await _tx.RollbackAsync(ct);
        }
        finally
        {
            await _tx.DisposeAsync();
            _tx = null;
        }
    }

    public Task ExecuteRawSql(string sql, CancellationToken ct = default)
    {
        return Db.Database.ExecuteSqlRawAsync(sql, ct);
    }

    public Task<List<TSqlResponse>> ExecuteRawSql<TSqlResponse>(string sql, CancellationToken ct = default)
        where TSqlResponse : class
    {
        return Db.Set<TSqlResponse>().FromSqlRaw(sql).ToListAsync(ct);
    }

    public Task<List<TSqlResponse>> ExecuteRawSql<TSqlResponse>(FormattableString sql, CancellationToken ct = default)
        where TSqlResponse : class
    {
        return Db.Set<TSqlResponse>().FromSql(sql).ToListAsync(ct);
    }

    public void Dispose()
    {
        _tx?.Dispose();
        Db.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>Create a repository instance for the given entity/key types.</summary>
    protected abstract IRepository<TEntity, TKey> CreateRepository<TEntity, TKey>()
        where TEntity : class, IEntity<TKey>, TMarker;

    // public void RollbackTrackedChanges()
    // {
    //     Db.ChangeTracker.Clear();
    // }
}
