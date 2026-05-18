using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Users.Queries;

public sealed class GetUserByIdQuery : IQuery<Result<UserDto>>
{
    public required string UserId { get; set; }
}
