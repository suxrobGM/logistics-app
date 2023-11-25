using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class RemoveRoleFromEmployeeCommand : IRequest<ResponseResult>
{
    public string UserId { get; set; } = default!;
    public string Role { get; set; } = default!;
}
