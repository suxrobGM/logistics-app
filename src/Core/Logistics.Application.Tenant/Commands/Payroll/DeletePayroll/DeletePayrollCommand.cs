using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class DeletePayrollCommand : IRequest<ResponseResult>
{
    public string Id { get; set; } = default!;
}
