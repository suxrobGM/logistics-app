using System.Linq.Expressions;
using Logistics.Domain.Core;
using Logistics.Domain.Entities;

namespace Logistics.Domain.Persistence;

/// <summary>
/// Tenant's repository.
/// </summary>
public interface ITenantRepository : IRepository
{
    /// <summary>
    /// Gets the current tenant data.
    /// </summary>
    Tenant GetCurrentTenant();

    /// <summary>
    /// Manually set the current tenant by its ID
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    void SetCurrentTenantById(string tenantId);
    
    /// <summary>
    /// Manually set the current tenant by directly passing the instance
    /// </summary>
    /// <param name="tenant">An instance of Tenant class</param>
    void SetCurrentTenant(Tenant tenant);

    /// <summary>
    /// Gets an entity object by its ID.
    /// </summary>
    /// <param name="id">Entity primary key</param>
    /// <typeparam name="TEntity">
    /// Class that implements <see cref="ITenantEntity"/> interfaces
    /// </typeparam>
    /// <returns>Entity object</returns>
    new Task<TEntity?> GetAsync<TEntity>(object? id)
        where TEntity : class, ITenantEntity;

    /// <summary>
    /// Gets an entity object by predicate.
    /// </summary>
    /// <param name="predicate">Predicate to filter query</param>
    /// <typeparam name="TEntity">
    /// Class that implements <see cref="ITenantEntity"/> interfaces
    /// </typeparam>
    /// <returns>Entity object</returns>
    new Task<TEntity?> GetAsync<TEntity>(Expression<Func<TEntity, bool>> predicate)
        where TEntity : class, ITenantEntity;

    /// <summary>
    /// Gets list of entity objects
    /// </summary>
    /// <param name="predicate">Predicate to filter query</param>
    /// <typeparam name="TEntity">
    /// Class that implements <see cref="ITenantEntity"/> interfaces
    /// </typeparam>
    /// <returns>List of entity objects</returns>
    new Task<List<TEntity>> GetListAsync<TEntity>(Expression<Func<TEntity, bool>>? predicate = default)
        where TEntity : class, ITenantEntity;

    /// <summary>
    /// Gets a dictionary of the entities and specified keys.
    /// </summary>
    /// <param name="keySelector">Key selector</param>
    /// <param name="predicate">Predicate to filter query</param>
    /// <typeparam name="TEntity">Class that implements the <see cref="ITenantEntity"/> interface</typeparam>
    /// <typeparam name="TKey">Key</typeparam>
    new Task<Dictionary<TKey, TEntity>> GetDictionaryAsync<TKey, TEntity>(
        Func<TEntity, TKey> keySelector,
        Expression<Func<TEntity, bool>>? predicate = default)
        where TEntity : class, ITenantEntity
        where TKey : notnull;

    /// <summary>
    /// Gets IQueryable entity objects.
    /// </summary>
    /// <param name="predicate">Predicate to filter query</param>
    /// <typeparam name="TEntity">
    /// Class that implements <see cref="ITenantEntity"/> interfaces
    /// </typeparam>
    /// <returns>IQueryable of entity objects</returns>
    new IQueryable<TEntity> Query<TEntity>(Expression<Func<TEntity, bool>>? predicate = default)
        where TEntity : class, ITenantEntity;

    /// <summary>
    /// Adds new entry to database.
    /// </summary>
    /// <param name="entity">Entity object</param>
    /// <typeparam name="TEntity">
    /// Class that implements <see cref="ITenantEntity"/> interfaces
    /// </typeparam>
    new Task AddAsync<TEntity>(TEntity entity)
        where TEntity : class, ITenantEntity;

    /// <summary>
    /// Updates existing entry.
    /// </summary>
    /// <param name="entity">Entity object</param>
    /// <typeparam name="TEntity">
    /// Class that implements <see cref="ITenantEntity"/> interfaces
    /// </typeparam>
    new void Update<TEntity>(TEntity entity)
        where TEntity : class, ITenantEntity;

    /// <summary>
    /// Deletes entity object from database.
    /// </summary>
    /// <param name="entity">Entity object</param>
    /// <typeparam name="TEntity">
    /// Class that implements <see cref="ITenantEntity"/> interfaces
    /// </typeparam>
    new void Delete<TEntity>(TEntity? entity)
        where TEntity : class, ITenantEntity;
}
