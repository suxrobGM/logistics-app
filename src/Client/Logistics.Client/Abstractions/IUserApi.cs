using Logistics.Models;

namespace Logistics.Client.Abstractions;

public interface IUserApi
{
    Task<ResponseResult<UserDto>> GetUserAsync(string id);
    Task<ResponseResult> UpdateUserAsync(UpdateUser user);
}
