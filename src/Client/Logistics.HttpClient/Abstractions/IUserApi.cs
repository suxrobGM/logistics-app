using Logistics.Shared.Models;

namespace Logistics.HttpClient.Abstractions;

public interface IUserApi
{
    Task<Result<UserDto>> GetUserAsync(Guid userId);
    Task<PagedResult<UserDto>> GetUsersAsync(SearchableQuery query);
    Task<Result> UpdateUserAsync(UpdateUser command);
    Task<Result<TenantDto>> GetUserCurrentTenant(Guid userId);
}
