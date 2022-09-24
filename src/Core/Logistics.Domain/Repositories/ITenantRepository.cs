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
    /// Gets an entity object by its ID.
    /// </summary>
    /// <param name="id">Entity primary key</param>
    /// <typeparam name="TEntity">
    /// Class that implements <see cref="IAggregateRoot"/> and <see cref="ITenantEntity"/> interfaces
    /// </typeparam>
    /// <returns>Entity object</returns>
    new Task<TEntity?> GetAsync<TEntity>(object? id) 
        where TEntity: class, IAggregateRoot, ITenantEntity;

    /// <summary>
    /// Gets an entity object by predicate.
    /// </summary>
    /// <param name="predicate">Predicate to filter query</param>
    /// <typeparam name="TEntity">
    /// Class that implements <see cref="IAggregateRoot"/> and <see cref="ITenantEntity"/> interfaces
    /// </typeparam>
    /// <returns>Entity object</returns>
    new Task<TEntity?> GetAsync<TEntity>(Expression<Func<TEntity, bool>> predicate) 
        where TEntity: class, IAggregateRoot, ITenantEntity;

    /// <summary>
    /// Gets list of entity objects
    /// </summary>
    /// <param name="predicate">Predicate to filter query</param>
    /// <typeparam name="TEntity">
    /// Class that implements <see cref="IAggregateRoot"/> and <see cref="ITenantEntity"/> interfaces
    /// </typeparam>
    /// <returns>List of entity objects</returns>
    new Task<List<TEntity>> GetListAsync<TEntity>(Expression<Func<TEntity, bool>>? predicate = default)
        where TEntity: class, IAggregateRoot, ITenantEntity;
    
    /// <summary>
    /// Gets a dictionary of the entities and specified keys.
    /// </summary>
    /// <param name="keySelector">Key selector</param>
    /// <param name="predicate">Predicate to filter query</param>
    /// <typeparam name="TEntity">Class that implements the <see cref="IAggregateRoot"/> interface</typeparam>
    /// <typeparam name="TKey">Key</typeparam>
    new Task<Dictionary<TKey, TEntity>> GetDictionaryAsync<TKey, TEntity>(
        Func<TEntity, TKey> keySelector,
        Expression<Func<TEntity, bool>>? predicate = default)
        where TEntity : class, IAggregateRoot, ITenantEntity
        where TKey : notnull;

    /// <summary>
    /// Gets IQueryable entity objects.
    /// </summary>
    /// <param name="predicate">Predicate to filter query</param>
    /// <typeparam name="TEntity">
    /// Class that implements <see cref="IAggregateRoot"/> and <see cref="ITenantEntity"/> interfaces
    /// </typeparam>
    /// <returns>IQueryable of entity objects</returns>
    new IQueryable<TEntity> Query<TEntity>(Expression<Func<TEntity, bool>>? predicate = default)
        where TEntity: class, IAggregateRoot, ITenantEntity;

    /// <summary>
    /// Adds new entry to database.
    /// </summary>
    /// <param name="entity">Entity object</param>
    /// <typeparam name="TEntity">
    /// Class that implements <see cref="IAggregateRoot"/> and <see cref="ITenantEntity"/> interfaces
    /// </typeparam>
    new Task AddAsync<TEntity>(TEntity entity)
        where TEntity: class, IAggregateRoot, ITenantEntity;

    /// <summary>
    /// Updates existing entry.
    /// </summary>
    /// <param name="entity">Entity object</param>
    /// <typeparam name="TEntity">
    /// Class that implements <see cref="IAggregateRoot"/> and <see cref="ITenantEntity"/> interfaces
    /// </typeparam>
    new void Update<TEntity>(TEntity entity)
        where TEntity: class, IAggregateRoot, ITenantEntity;

    /// <summary>
    /// Deletes entity object from database.
    /// </summary>
    /// <param name="entity">Entity object</param>
    /// <typeparam name="TEntity">
    /// Class that implements <see cref="IAggregateRoot"/> and <see cref="ITenantEntity"/> interfaces
    /// </typeparam>
    new void Delete<TEntity>(TEntity? entity)
        where TEntity: class, IAggregateRoot, ITenantEntity;
}