using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Identity.Users.Queries;

public sealed class GetUserByIdQuery : IQuery<Result<UserDto>>
{
    public required string UserId { get; set; }
}
