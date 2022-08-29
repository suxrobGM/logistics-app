using System.Linq.Expressions;

namespace Logistics.Domain.Repositories;

/// <summary>
/// Tenant's repository.
/// </summary>
public interface ITenantRepository : IRepository
{
    /// <summary>
    /// Gets the current tenant data.
    /// </summary>
    Tenant? CurrentTenant { get; }
    
    /// <summary>
    /// Get entity object by its ID.
    /// </summary>
    /// <param name="id">Entity primary key</param>
    /// <typeparam name="TEntity">
    /// Class that implements <see cref="IAggregateRoot"/> and <see cref="ITenantEntity"/> interfaces
    /// </typeparam>
    /// <returns>Entity object</returns>
    new Task<TEntity?> GetAsync<TEntity>(object? id) 
        where TEntity: class, IAggregateRoot, ITenantEntity;

    /// <summary>
    /// Get entity object by predicate.
    /// </summary>
    /// <param name="predicate">Predicate to filter query</param>
    /// <typeparam name="TEntity">
    /// Class that implements <see cref="IAggregateRoot"/> and <see cref="ITenantEntity"/> interfaces
    /// </typeparam>
    /// <returns>Entity object</returns>
    new Task<TEntity?> GetAsync<TEntity>(Expression<Func<TEntity, bool>> predicate) 
        where TEntity: class, IAggregateRoot, ITenantEntity;

    /// <summary>
    /// Get list of entity objects
    /// </summary>
    /// <param name="predicate">Predicate to filter query</param>
    /// <typeparam name="TEntity">
    /// Class that implements <see cref="IAggregateRoot"/> and <see cref="ITenantEntity"/> interfaces
    /// </typeparam>
    /// <returns>List of entity objects</returns>
    new Task<IList<TEntity>> GetListAsync<TEntity>(Expression<Func<TEntity, bool>> predicate = null!)
        where TEntity: class, IAggregateRoot, ITenantEntity;

    /// <summary>
    /// Get IQueryable entity objects.
    /// </summary>
    /// <param name="predicate">Predicate to filter query</param>
    /// <typeparam name="TEntity">
    /// Class that implements <see cref="IAggregateRoot"/> and <see cref="ITenantEntity"/> interfaces
    /// </typeparam>
    /// <returns>IQueryable of entity objects</returns>
    new IQueryable<TEntity> GetQuery<TEntity>(Expression<Func<TEntity, bool>> predicate = null!)
        where TEntity: class, IAggregateRoot, ITenantEntity;

    /// <summary>
    /// Add new entry to database.
    /// </summary>
    /// <param name="entity">Entity object</param>
    /// <typeparam name="TEntity">
    /// Class that implements <see cref="IAggregateRoot"/> and <see cref="ITenantEntity"/> interfaces
    /// </typeparam>
    new Task AddAsync<TEntity>(TEntity entity)
        where TEntity: class, IAggregateRoot, ITenantEntity;

    /// <summary>
    /// Update existing entry.
    /// </summary>
    /// <param name="entity">Entity object</param>
    /// <typeparam name="TEntity">
    /// Class that implements <see cref="IAggregateRoot"/> and <see cref="ITenantEntity"/> interfaces
    /// </typeparam>
    new void Update<TEntity>(TEntity entity)
        where TEntity: class, IAggregateRoot, ITenantEntity;

    /// <summary>
    /// Delete entity object from database.
    /// </summary>
    /// <param name="entity">Entity object</param>
    /// <typeparam name="TEntity">
    /// Class that implements <see cref="IAggregateRoot"/> and <see cref="ITenantEntity"/> interfaces
    /// </typeparam>
    new void Delete<TEntity>(TEntity? entity)
        where TEntity: class, IAggregateRoot, ITenantEntity;
}