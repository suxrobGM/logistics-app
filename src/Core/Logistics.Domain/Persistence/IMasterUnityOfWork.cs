using Logistics.Domain.Core;

namespace Logistics.Domain.Persistence;

public interface IMasterUnityOfWork : IDisposable
{
    IMasterRepository2<TEntity> Repository<TEntity>() where TEntity : class, IEntity<string>;
    
    /// <summary>
    /// Save changes to database
    /// </summary>
    /// <returns>Number of rows modified after save changes.</returns>
    Task<int> CommitAsync();
}
