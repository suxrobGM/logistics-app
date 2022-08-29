namespace Logistics.Domain.Repositories;

/// <summary>
/// Generic repository.
/// </summary>

public interface IRepository
{
    /// <summary>
    /// Unit of work
    /// </summary>
    IUnitOfWork UnitOfWork { get; }
    
    /// <summary>
    /// Get entity object by its ID.
    /// </summary>
    /// <param name="id">Entity primary key</param>
    /// <typeparam name="TEntity">Class that implements the <see cref="IAggregateRoot"/> interface</typeparam>
    /// <returns>Entity object</returns>
    Task<TEntity?> GetAsync<TEntity>(object? id) 
        where TEntity: class, IAggregateRoot;

    /// <summary>
    /// Get entity object by predicate.
    /// </summary>
    /// <param name="predicate">Predicate to filter query</param>
    /// <typeparam name="TEntity">Class that implements the <see cref="IAggregateRoot"/> interface</typeparam>
    /// <returns>Entity object</returns>
    Task<TEntity?> GetAsync<TEntity>(Expression<Func<TEntity, bool>> predicate) 
        where TEntity: class, IAggregateRoot;

    /// <summary>
    /// Get list of entity objects
    /// </summary>
    /// <param name="predicate">Predicate to filter query</param>
    /// <typeparam name="TEntity">Class that implements the <see cref="IAggregateRoot"/> interface</typeparam>
    /// <returns>List of entity objects</returns>
    Task<IList<TEntity>> GetListAsync<TEntity>(Expression<Func<TEntity, bool>>? predicate = default)
        where TEntity: class, IAggregateRoot;

    /// <summary>
    /// Get IQueryable entity objects.
    /// </summary>
    /// <param name="predicate">Predicate to filter query</param>
    /// <typeparam name="TEntity">Class that implements the <see cref="IAggregateRoot"/> interface</typeparam>
    /// <returns>IQueryable of entity objects</returns>
    IQueryable<TEntity> Query<TEntity>(Expression<Func<TEntity, bool>>? predicate = default)
        where TEntity: class, IAggregateRoot;

    /// <summary>
    /// Add new entry to database.
    /// </summary>
    /// <param name="entity">Entity object</param>
    /// <typeparam name="TEntity">Class that implements the <see cref="IAggregateRoot"/> interface</typeparam>
    Task AddAsync<TEntity>(TEntity entity)
        where TEntity: class, IAggregateRoot;

    /// <summary>
    /// Update existing entry.
    /// </summary>
    /// <param name="entity">Entity object</param>
    /// <typeparam name="TEntity">Class that implements the <see cref="IAggregateRoot"/> interface</typeparam>
    void Update<TEntity>(TEntity entity)
        where TEntity: class, IAggregateRoot;

    /// <summary>
    /// Delete entity object from database.
    /// </summary>
    /// <param name="entity">Entity object</param>
    /// <typeparam name="TEntity">Class that implements the <see cref="IAggregateRoot"/> interface</typeparam>
    void Delete<TEntity>(TEntity? entity)
        where TEntity: class, IAggregateRoot;
}