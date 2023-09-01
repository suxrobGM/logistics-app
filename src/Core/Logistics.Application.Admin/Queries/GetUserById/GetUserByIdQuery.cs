using Logistics.Models;

namespace Logistics.Application.Admin.Queries;

public sealed class GetUserByIdQuery : Request<ResponseResult<UserDto>>
{
    public required string UserId { get; set; }
}
