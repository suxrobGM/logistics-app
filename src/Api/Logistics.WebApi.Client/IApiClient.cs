namespace Logistics.WebApi.Client;

public interface IApiClient
{
    Task CreateUserAsync(UserDto userDto);
    Task<bool> TryCreateUserAsync(UserDto userDto);
    Task<bool> UserExistsAsync(string externalId);
}
