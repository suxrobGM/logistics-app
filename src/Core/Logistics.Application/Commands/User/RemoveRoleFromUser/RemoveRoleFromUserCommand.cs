using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class RemoveRoleFromUserCommand : IAppRequest
{
    public string? UserId { get; set; }
    public string? Role { get; set; }
}
