using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class DeleteCustomerCommand : IRequest<ResponseResult>
{
    public string Id { get; set; } = default!;
}
