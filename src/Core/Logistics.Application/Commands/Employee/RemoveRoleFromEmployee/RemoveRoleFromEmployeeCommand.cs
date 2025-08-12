using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class RemoveRoleFromEmployeeCommand : IAppRequest
{
    public Guid UserId { get; set; }
    public string Role { get; set; } = null!;
}
