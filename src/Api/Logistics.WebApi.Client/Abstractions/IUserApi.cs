namespace Logistics.WebApi.Client;

public interface IUserApi
{
    Task CreateUserAsync(UserDto user);
    Task<PagedDataResult<UserDto>> GetUsersAsync(int page = 1, int pageSize = 10);
    Task<bool> TryCreateUserAsync(UserDto user);
    Task<bool> UserExistsAsync(string externalId);
}
