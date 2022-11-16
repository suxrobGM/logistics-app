using Logistics.Infrastructure.EF.Data;

namespace Logistics.Infrastructure.EF.Persistence;

internal class MainRepository: GenericRepository<MainDbContext>, IMainRepository
{
    public MainRepository(
        MainDbContext context,
        UnitOfWork<MainDbContext> unitOfWork) : base(context, unitOfWork)
    {
    }
}