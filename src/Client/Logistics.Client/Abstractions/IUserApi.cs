using Logistics.Client.Models;

namespace Logistics.Client.Abstractions;

public interface IUserApi
{
    Task<ResponseResult<User>> GetUserAsync(string id);
    Task<ResponseResult> UpdateUserAsync(UpdateUser user);
}
