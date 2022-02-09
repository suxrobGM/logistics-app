namespace Logistics.Infrastructure.Repositories;

public class CargoRepository : Repository<Cargo>, ICargoRepository
{
    protected CargoRepository(DatabaseContext context, IUnitOfWork unitOfWork) 
        : base(context, unitOfWork)
    {
    }
}