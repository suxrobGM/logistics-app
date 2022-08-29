namespace Logistics.EntityFramework.Repositories;

internal class MainRepository: GenericRepository<MainDbContext>, IMainRepository
{
    public MainRepository(
        MainDbContext context,
        UnitOfWork<MainDbContext> unitOfWork) : base(context, unitOfWork)
    {
    }
}