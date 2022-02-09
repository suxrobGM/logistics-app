namespace Logistics.Infrastructure.Repositories;

public class TruckRepository : Repository<Truck>, ITruckRepository
{
    protected TruckRepository(DatabaseContext context, IUnitOfWork unitOfWork) 
        : base(context, unitOfWork)
    {
    }
}