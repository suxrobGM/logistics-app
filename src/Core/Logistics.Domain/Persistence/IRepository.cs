using System.Linq.Expressions;
using Logistics.Domain.Core;
using Logistics.Domain.Specifications;

namespace Logistics.Domain.Persistence;

/// <summary>
/// Generic repository.
/// </summary>
/// <typeparam name="TEntity">Class that implements the <see cref="IEntity{TKey}"/> interface</typeparam>
/// <typeparam name="TEntityKey">The data type of entity primary key</typeparam> 
public interface IRepository<TEntity, in TEntityKey> where TEntity : class, IEntity<TEntityKey>
{
    IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> specification);

    IQueryable<TEntity> Query();
    
    /// <summary>
    /// Asynchronously counts number of entities.
    /// </summary>
    /// <param name="predicate">Predicate to filter query</param>
    /// <returns>Number of elements that satisfies the specified condition</returns>
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null);
    
    /// <summary>
    /// Gets an entity object by ID.
    /// </summary>
    /// <param name="id">Entity primary key</param>
    /// <returns>Entity object</returns>
    Task<TEntity?> GetByIdAsync(TEntityKey id);

    /// <summary>
    /// Gets an entity object filtered by predicate.
    /// </summary>
    /// <param name="predicate">Predicate to filter query</param>
    /// <returns>Entity object</returns>
    Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Gets a list of the entity objects
    /// </summary>
    /// <param name="predicate">Predicate to filter query</param>
    /// <returns>List of entity objects</returns>
    Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate);
    
    /// <summary>
    /// Gets a list of the entity objects
    /// </summary>
    /// <param name="specification">Specification</param>
    /// <returns>List of entity objects</returns>
    Task<List<TEntity>> GetListAsync(ISpecification<TEntity>? specification = null);

    /// <summary>
    /// Adds new entry to database.
    /// </summary>
    /// <param name="entity">Entity object</param>
    Task AddAsync(TEntity entity);

    /// <summary>
    /// Updates existing entry.
    /// </summary>
    /// <param name="entity">Entity object</param>
    void Update(TEntity entity);

    /// <summary>
    /// Deletes an entity object from the database.
    /// </summary>
    /// <param name="entity">Entity object</param>
    void Delete(TEntity? entity);
}
