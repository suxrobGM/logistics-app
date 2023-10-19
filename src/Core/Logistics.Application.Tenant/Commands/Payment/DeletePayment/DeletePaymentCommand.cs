using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class DeletePaymentCommand : IRequest<ResponseResult>
{
    public string Id { get; set; } = default!;
}
