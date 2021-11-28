using Logistics.Domain.Common;
using Logistics.Domain.TruckAggregate;
using Logistics.Infrastructure.Data;

namespace Logistics.Infrastructure.Repositories;

public class TruckRepository : Repository<Truck>
{
    protected TruckRepository(DatabaseContext context, IUnitOfWork unitOfWork) 
        : base(context, unitOfWork)
    {
    }
}