namespace Logistics.WebApi.Client;

public interface IUserApi
{
    Task<UserDto> GetUserAsync(string id);
    Task<UserDto> GetUserByExternalIdAsync(string externalId);
    Task<PagedDataResult<UserDto>> GetUsersAsync(int page = 1, int pageSize = 10);
    Task<bool> UserExistsAsync(string externalId);
    Task CreateUserAsync(UserDto user);
    Task UpdateUserAsync(UserDto user);
    Task<bool> TryCreateUserAsync(UserDto user);
}
