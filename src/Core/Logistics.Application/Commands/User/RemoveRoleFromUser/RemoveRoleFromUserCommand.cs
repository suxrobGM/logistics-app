using Logistics.Shared.Models;

using MediatR;

namespace Logistics.Application.Commands;

public class RemoveRoleFromUserCommand : IRequest<Result>
{
    public string? UserId { get; set; }
    public string? Role { get; set; }
}
