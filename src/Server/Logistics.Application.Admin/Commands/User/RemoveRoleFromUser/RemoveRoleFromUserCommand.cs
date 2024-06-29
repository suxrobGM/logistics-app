using Logistics.Application.Core;
using Logistics.Shared;
using MediatR;

namespace Logistics.Application.Admin.Commands;

public class RemoveRoleFromUserCommand : IRequest<Result>
{
    public string? UserId { get; set; }
    public string? Role { get; set; }
}
