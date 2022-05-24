namespace Logistics.Domain.Repositories;

/// <summary>
/// Application's main repository.
/// </summary>
/// <typeparam name="TEntity">Class that implements IAggregateRoot interface</typeparam>
public interface IMainRepository<TEntity> : IRepository<TEntity>
    where TEntity : class, IAggregateRoot
{
    /// <summary>
    /// Unit of work
    /// </summary>
    IMainUnitOfWork UnitOfWork { get; }
}