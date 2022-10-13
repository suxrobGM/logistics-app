namespace Logistics.Sdk.Abstractions;

internal interface IUserApi
{
    Task<ResponseResult<User>> GetUserAsync(string id);
    Task<ResponseResult> UpdateUser(UpdateUser user);
}
