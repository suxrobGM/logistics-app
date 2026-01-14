using Logistics.Shared.Models;

namespace Logistics.HttpClient.Abstractions;

public interface IUserApi
{
    Task<UserDto?> GetUserAsync(Guid userId);
    Task<PagedResponse<UserDto>?> GetUsersAsync(SearchableQuery query);
    Task<bool> UpdateUserAsync(UpdateUser command);
    Task<TenantDto?> GetUserCurrentTenant(Guid userId);
    Task<string[]?> GetCurrentUserPermissionsAsync();
}
