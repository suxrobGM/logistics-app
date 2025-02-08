using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Commands;

public class RemoveRoleFromEmployeeCommand : IRequest<Result>
{
    public string UserId { get; set; } = null!;
    public string Role { get; set; } = null!;
}
