using Logistics.Domain.Entities;
using Logistics.Domain.Repositories;
using Logistics.EntityFramework.Data;

namespace Logistics.EntityFramework.Repositories;

public class TruckRepository : Repository<Truck>, ITruckRepository
{
    protected TruckRepository(DatabaseContext context, IUnitOfWork unitOfWork) 
        : base(context, unitOfWork)
    {
    }
}