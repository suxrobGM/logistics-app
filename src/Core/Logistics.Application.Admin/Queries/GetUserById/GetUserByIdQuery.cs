using Logistics.Shared.Models;
using Logistics.Shared;
using MediatR;

namespace Logistics.Application.Admin.Queries;

public sealed class GetUserByIdQuery : IRequest<ResponseResult<UserDto>>
{
    public required string UserId { get; set; }
}
