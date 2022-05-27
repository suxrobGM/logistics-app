namespace Logistics.EntityFramework.Repositories;

internal class MainRepository<TEntity> : GenericRepository<TEntity, MainDbContext>, IMainRepository<TEntity>
    where TEntity : class, IAggregateRoot
{
    public MainRepository(
        MainDbContext context,
        IMainUnitOfWork unitOfWork) : base(context)
    {
        UnitOfWork = unitOfWork;
    }

    public IMainUnitOfWork UnitOfWork { get; }
}