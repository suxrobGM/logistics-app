using Logistics.Domain.Entities;
using Logistics.Domain.Repositories;
using Logistics.EntityFramework.Data;

namespace Logistics.EntityFramework.Repositories;

public class CargoRepository : Repository<Cargo>, ICargoRepository
{
    protected CargoRepository(DatabaseContext context, IUnitOfWork unitOfWork) 
        : base(context, unitOfWork)
    {
    }
}