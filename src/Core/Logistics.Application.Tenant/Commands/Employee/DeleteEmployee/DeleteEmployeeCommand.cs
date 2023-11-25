using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class DeleteEmployeeCommand : IRequest<ResponseResult>
{
    public string UserId { get; set; } = default!;
}
