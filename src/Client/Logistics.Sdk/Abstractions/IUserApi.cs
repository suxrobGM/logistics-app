namespace Logistics.Sdk.Abstractions;

public interface IUserApi
{
    Task<ResponseResult<User>> GetUserAsync(string id);
    Task<ResponseResult> UpdateUser(UpdateUser user);
}
