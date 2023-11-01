using Logistics.Domain.Persistence;
using Logistics.Infrastructure.EF.Data;

namespace Logistics.Infrastructure.EF.Persistence;

internal class MasterRepository: GenericRepository<MasterDbContext>, IMasterRepository
{
    public MasterRepository(
        MasterDbContext context,
        UnitOfWork<MasterDbContext> unitOfWork) : base(context, unitOfWork)
    {
    }
}
