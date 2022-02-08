using System.Linq.Expressions;

namespace Logistics.Domain;

/// <summary>
/// Generic repository.
/// </summary>
/// <typeparam name="TEntity">Class that implements IAggregateRoot interface</typeparam>
public interface IRepository<TEntity> where TEntity : class, IAggregateRoot
{
    /// <summary>
    /// Unit of work
    /// </summary>
    IUnitOfWork UnitOfWork { get; }
    
    /// <summary>
    /// Get entity object by its ID.
    /// </summary>
    /// <param name="id">Entity primary key</param>
    /// <returns>Entity object</returns>
    TEntity GetById(string id);
        
    /// <summary>
    /// Get entity object by predicate.
    /// </summary>
    /// <param name="predicate">Predicate to filter query</param>
    /// <returns>Entity object</returns>
    TEntity Get(Expression<Func<TEntity, bool>> predicate);
        
    /// <summary>
    /// Get list of entity objects
    /// </summary>
    /// <param name="predicate">Predicate to filter query</param>
    /// <returns>List of entity objects</returns>
    List<TEntity> GetList(Expression<Func<TEntity, bool>> predicate = null!);
        
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
    void Add(TEntity entity);
        
    /// <summary>
    /// Update existing entry.
    /// </summary>
    /// <param name="entity">Entity object</param>
    void Update(TEntity entity);
        
    /// <summary>
    /// Delete entry from database using its ID as a primary key.
    /// </summary>
    /// <param name="id">Primary key of entity</param>
    void Delete(string id);
        
    /// <summary>
    /// Delete entity object from database.
    /// </summary>
    /// <param name="entity">Entity object</param>
    void Delete(TEntity entity);
}