using Logistics.Domain.Core;

namespace Logistics.Domain.Persistence;

/// <summary>
///     Unit of Work for master (cross-tenant) entities.
/// </summary>
public interface IMasterUnitOfWork : IUnitOfWork<IMasterEntity>
{
    /// <inheritdoc cref="IUnitOfWork{TMarker}.Repository{TEntity}" />
    new IMasterRepository<TEntity, Guid> Repository<TEntity>()
        where TEntity : class, IEntity<Guid>, IMasterEntity;

    /// <inheritdoc cref="IUnitOfWork{TMarker}.Repository{TEntity, TKey}" />
    new IMasterRepository<TEntity, TKey> Repository<TEntity, TKey>()
        where TEntity : class, IEntity<TKey>, IMasterEntity;
}
