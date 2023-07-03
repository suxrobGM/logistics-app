namespace Logistics.Domain.Persistence;

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
    /// Asynchronously counts number of entities.
    /// </summary>
    /// <param name="predicate">Predicate to filter query</param>
    /// <typeparam name="TEntity">Class that implements the <see cref="IEntity{TKey}"/> interface</typeparam>
    /// <returns>Number of elements that satisfies the specified condition</returns>
    Task<int> CountAsync<TEntity>(Expression<Func<TEntity, bool>>? predicate = default)
        where TEntity: class, IEntity<string>;
    
    /// <summary>
    /// Asynchronously computes the sum of a sequence of values
    /// </summary>
    /// <param name="selector">A projection function to apply to each element</param>
    /// <typeparam name="TEntity">Class that implements the <see cref="IEntity{TKey}"/> interface</typeparam>
    /// <returns>The sum of the projected values</returns>
    Task<double> SumAsync<TEntity>(Expression<Func<TEntity, double>> selector)
        where TEntity: class, IEntity<string>;
    
    /// <summary>
    /// Gets an entity object by ID.
    /// </summary>
    /// <param name="id">Entity primary key</param>
    /// <typeparam name="TEntity">Class that implements the <see cref="IEntity{TKey}"/> interface</typeparam>
    /// <returns>Entity object</returns>
    Task<TEntity?> GetAsync<TEntity>(object? id) 
        where TEntity: class, IEntity<string>;

    /// <summary>
    /// Gets an entity object filtered by predicate.
    /// </summary>
    /// <param name="predicate">Predicate to filter query</param>
    /// <typeparam name="TEntity">Class that implements the <see cref="IEntity{TKey}"/> interface</typeparam>
    /// <returns>Entity object</returns>
    Task<TEntity?> GetAsync<TEntity>(Expression<Func<TEntity, bool>> predicate) 
        where TEntity: class, IEntity<string>;

    /// <summary>
    /// Gets a list of the entity objects
    /// </summary>
    /// <param name="predicate">Predicate to filter query</param>
    /// <typeparam name="TEntity">Class that implements the <see cref="IEntity{TKey}"/> interface</typeparam>
    /// <returns>List of entity objects</returns>
    Task<List<TEntity>> GetListAsync<TEntity>(Expression<Func<TEntity, bool>>? predicate = default)
        where TEntity: class, IEntity<string>;
    
    /// <summary>
    /// Gets a dictionary of the entities and specified keys.
    /// </summary>
    /// <param name="keySelector">Key selector</param>
    /// <param name="predicate">Predicate to filter query</param>
    /// <typeparam name="TEntity">Class that implements the <see cref="IEntity{TKey}"/> interface</typeparam>
    /// <typeparam name="TKey">Key</typeparam>
    Task<Dictionary<TKey, TEntity>> GetDictionaryAsync<TKey, TEntity>(
        Func<TEntity, TKey> keySelector,
        Expression<Func<TEntity, bool>>? predicate = default)
        where TEntity : class, IEntity<string>
        where TKey : notnull;

    /// <summary>
    /// Gets IQueryable entity objects.
    /// </summary>
    /// <param name="predicate">Predicate to filter query</param>
    /// <typeparam name="TEntity">Class that implements the <see cref="IEntity{TKey}"/> interface</typeparam>
    /// <returns>IQueryable of entity objects</returns>
    IQueryable<TEntity> Query<TEntity>(Expression<Func<TEntity, bool>>? predicate = default)
        where TEntity: class, IEntity<string>;

    /// <summary>
    /// Adds new entry to database.
    /// </summary>
    /// <param name="entity">Entity object</param>
    /// <typeparam name="TEntity">Class that implements the <see cref="IEntity{TKey}"/> interface</typeparam>
    Task AddAsync<TEntity>(TEntity entity)
        where TEntity: class, IEntity<string>;

    /// <summary>
    /// Updates existing entry.
    /// </summary>
    /// <param name="entity">Entity object</param>
    /// <typeparam name="TEntity">Class that implements the <see cref="IEntity{TKey}"/> interface</typeparam>
    void Update<TEntity>(TEntity entity)
        where TEntity: class, IEntity<string>;

    /// <summary>
    /// Deletes entity object from database.
    /// </summary>
    /// <param name="entity">Entity object</param>
    /// <typeparam name="TEntity">Class that implements the <see cref="IEntity{TKey}"/> interface</typeparam>
    void Delete<TEntity>(TEntity? entity)
        where TEntity: class, IEntity<string>;
}