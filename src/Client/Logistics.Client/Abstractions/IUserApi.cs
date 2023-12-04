using Logistics.Client.Models;
using Logistics.Shared;
using Logistics.Shared.Models;

namespace Logistics.Client.Abstractions;

public interface IUserApi
{
    Task<ResponseResult<UserDto>> GetUserAsync(string userId);
    Task<PagedResponseResult<UserDto>> GetUsersAsync(SearchableQuery query);
    Task<ResponseResult> UpdateUserAsync(UpdateUser command);
    Task<ResponseResult<OrganizationDto[]>> GetUserOrganizations(string userId);
}
