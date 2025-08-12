using Logistics.Domain.Core;

namespace Logistics.Domain.Persistence;

public interface IMasterUnityOfWork : IDisposable
{
    IMasterRepository<TEntity, Guid> Repository<TEntity>() where TEntity : class, IEntity<Guid>, IMasterEntity;

    IMasterRepository<TEntity, TKey> Repository<TEntity, TKey>() where TEntity : class, IEntity<TKey>, IMasterEntity;

    /// <summary>
    ///     Save changes to the database
    /// </summary>
    /// <returns>Number of rows modified after save changes.</returns>
    Task<int> SaveChangesAsync();
}
