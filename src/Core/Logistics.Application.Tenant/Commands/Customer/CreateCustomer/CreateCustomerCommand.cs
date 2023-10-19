using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class CreateCustomerCommand : IRequest<ResponseResult>
{
    public string Name { get; set; } = default!;
}
