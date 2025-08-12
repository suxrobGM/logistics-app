using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public sealed class GetUserByIdQuery : IAppRequest<Result<UserDto>>
{
    public required string UserId { get; set; }
}
