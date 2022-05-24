using System.Linq.Expressions;

namespace Logistics.Domain.Repositories;

/// <summary>
/// Tenant's repository.
/// </summary>
/// <typeparam name="TEntity">Class that implements IAggregateRoot interface</typeparam>
public interface ITenantRepository<TEntity> where TEntity : class, IAggregateRoot, ITenantEntity
{
    /// <summary>
    /// Unit of work
    /// </summary>
    ITenantUnitOfWork UnitOfWork { get; }

    /// <summary>
    /// Get entity object by its ID.
    /// </summary>
    /// <param name="id">Entity primary key</param>
    /// <returns>Entity object</returns>
    Task<TEntity?> GetAsync(object id);

    /// <summary>
    /// Get entity object by predicate.
    /// </summary>
    /// <param name="predicate">Predicate to filter query</param>
    /// <returns>Entity object</returns>
    Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Get list of entity objects
    /// </summary>
    /// <param name="predicate">Predicate to filter query</param>
    /// <returns>List of entity objects</returns>
    Task<IList<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate = null!);

    /// <summary>
    /// Get IQueryable entity objects.
    /// </summary>
    /// <param name="predicate">Predicate to filter query</param>
    /// <returns>IQueryable of entity objects</returns>
    IQueryable<TEntity> GetQuery(Expression<Func<TEntity, bool>> predicate = null!);

    /// <summary>
    /// Add new entry to database.
    /// </summary>
    /// <param name="entity">Entity object</param>
    Task AddAsync(TEntity entity);

    /// <summary>
    /// Update existing entry.
    /// </summary>
    /// <param name="entity">Entity object</param>
    void Update(TEntity entity);

    /// <summary>
    /// Delete entry from database using its ID as a primary key.
    /// </summary>
    /// <param name="id">Primary key of entity</param>
    void Delete(object id);

    /// <summary>
    /// Delete entity object from database.
    /// </summary>
    /// <param name="entity">Entity object</param>
    void Delete(TEntity? entity);
}