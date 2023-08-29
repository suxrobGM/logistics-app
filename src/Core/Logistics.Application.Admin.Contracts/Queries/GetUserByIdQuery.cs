using Logistics.Models;

namespace Logistics.Application.Admin.Queries;

public sealed class GetUserByIdQuery : RequestBase<ResponseResult<UserDto>>
{
    public required string UserId { get; set; }
}
