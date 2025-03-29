using Logistics.Domain.Core;

namespace Logistics.Domain.Persistence;

/// <summary>
/// Master repository.
/// </summary>
/// <typeparam name="TEntity">Class that implements the <see cref="IEntity{TKey}"/> interface</typeparam> 
//public interface IMasterRepository<TEntity> : IRepository<TEntity, string> where TEntity : class, IEntity<string>;

/// <summary>
/// Master repository.
/// </summary>
/// <typeparam name="TEntity">Class that implements the <see cref="IEntity{TKey}"/> interface</typeparam> 
public interface IMasterRepository<TEntity, in TKey> : IRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>;