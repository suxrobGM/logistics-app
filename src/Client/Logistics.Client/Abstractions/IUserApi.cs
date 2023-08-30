using Logistics.Models;

namespace Logistics.Client.Abstractions;

public interface IUserApi
{
    Task<ResponseResult<UserDto>> GetUserAsync(string userId);
    Task<ResponseResult> UpdateUserAsync(UpdateUser user);
    Task<ResponseResult<OrganizationDto[]>> GetUserOrganizations(string userId);
}
