using Logistics.Domain.Core;

namespace Logistics.Domain.Persistence;

/// <summary>
/// Master repository.
/// </summary>
/// <typeparam name="TEntity">Class that implements the <see cref="IEntity{TKey}"/> interface</typeparam>
/// <typeparam name="TKey">Data type of primary key</typeparam> 
public interface IMasterRepository<TEntity, in TKey> : IRepository<TEntity, TKey> 
    where TEntity : class, IEntity<TKey>, IMasterEntity;