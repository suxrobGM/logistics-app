using Logistics.Domain.Entities;
using Logistics.Domain.Repositories;
using Logistics.EntityFramework.Data;

namespace Logistics.EntityFramework.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(DatabaseContext context, IUnitOfWork unitOfWork) 
        : base(context, unitOfWork)
    {
    }
}