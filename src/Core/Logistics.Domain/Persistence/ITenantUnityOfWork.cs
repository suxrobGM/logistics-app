using Logistics.Domain.Core;

namespace Logistics.Domain.Persistence;

public interface ITenantUnityOfWork : IDisposable
{
    ITenantRepository2<TEntity> Repository<TEntity>() where TEntity : class, ITenantEntity;
    
    /// <summary>
    /// Save changes to database
    /// </summary>
    /// <returns>Number of rows modified after save changes.</returns>
    Task<int> CommitAsync();
}
