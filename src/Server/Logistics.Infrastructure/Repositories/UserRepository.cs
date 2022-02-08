using Logistics.Domain;
using Logistics.Domain.UserAggregate;
using Logistics.Infrastructure.Data;

namespace Logistics.Infrastructure.Repositories;

public class UserRepository : Repository<User>
{
    public UserRepository(DatabaseContext context, IUnitOfWork unitOfWork) 
        : base(context, unitOfWork)
    {
    }
}