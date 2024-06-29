namespace Logistics.Domain.Services;

public interface IUserService
{
    Task UpdateUserAsync(UpdateUserData userData);
}
