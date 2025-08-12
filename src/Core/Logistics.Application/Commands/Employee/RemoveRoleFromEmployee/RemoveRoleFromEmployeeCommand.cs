using Logistics.Shared.Models;

using MediatR;

namespace Logistics.Application.Commands;

public class RemoveRoleFromEmployeeCommand : IRequest<Result>
{
    public Guid UserId { get; set; }
    public string Role { get; set; } = null!;
}
