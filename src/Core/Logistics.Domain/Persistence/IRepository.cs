using System.Linq.Expressions;
using Logistics.Domain.Core;
using Logistics.Domain.Specifications;

namespace Logistics.Domain.Persistence;

/// <summary>
///     Generic repository.
/// </summary>
/// <typeparam name="TEntity">Class that implements the <see cref="IEntity{TKey}" /> interface</typeparam>
/// <typeparam name="TEntityKey">The data type of entity primary key</typeparam>
public interface IRepository<TEntity, in TEntityKey> where TEntity : class, IEntity<TEntityKey>
{
    /// <summary>
    ///     Applies a specification to the entity query.
    /// </summary>
    /// <param name="specification"></param>
    /// <returns></returns>
    IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> specification);

    /// <summary>
    ///     Direct access to a queryable entity set. Not recommended for general use.
    ///     Instead, use <see cref="ApplySpecification" /> method or other repository methods.
    /// </summary>
    /// <returns></returns>
    IQueryable<TEntity> Query();

    /// <summary>
    ///     Asynchronously counts the number of entities.
    /// </summary>
    /// <param name="predicate">Predicate to filter query</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Number of elements that satisfies the specified condition</returns>
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken ct = default);

    /// <summary>
    ///     Gets an entity object by ID.
    /// </summary>
    /// <param name="id">Entity primary key</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Entity object</returns>
    Task<TEntity?> GetByIdAsync(TEntityKey id, CancellationToken ct = default);

    /// <summary>
    ///     Gets an entity object filtered by predicate.
    /// </summary>
    /// <param name="predicate">Predicate to filter query</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Entity object</returns>
    Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);

    /// <summary>
    ///     Gets a list of the entity objects
    /// </summary>
    /// <param name="predicate">Predicate to filter query</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of entity objects</returns>
    Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);

    /// <summary>
    ///     Gets a list of the entity objects
    /// </summary>
    /// <param name="specification">Specification</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of entity objects</returns>
    Task<List<TEntity>> GetListAsync(ISpecification<TEntity>? specification = null, CancellationToken ct = default);

    /// <summary>
    ///     Adds new entry to database.
    /// </summary>
    /// <param name="entity">Entity object</param>
    /// <param name="ct">Cancellation token</param>
    Task AddAsync(TEntity entity, CancellationToken ct = default);

    /// <summary>
    ///     Batch adds new entries to database.
    /// </summary>
    /// <param name="entities">Entity objects</param>
    /// <param name="ct">Cancellation token</param>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);

    /// <summary>
    ///     Updates existing entry.
    /// </summary>
    /// <param name="entity">Entity object</param>
    void Update(TEntity entity);

    /// <summary>
    ///     Deletes an entity object from the database.
    /// </summary>
    /// <param name="entity">Entity object</param>
    void Delete(TEntity? entity);
}
