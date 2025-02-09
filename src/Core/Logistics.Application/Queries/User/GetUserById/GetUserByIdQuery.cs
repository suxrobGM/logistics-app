using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Queries;

public sealed class GetUserByIdQuery : IRequest<Result<UserDto>>
{
    public required string UserId { get; set; }
}
