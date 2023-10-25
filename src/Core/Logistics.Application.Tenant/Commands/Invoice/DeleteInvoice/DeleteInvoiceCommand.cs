using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class DeleteInvoiceCommand : IRequest<ResponseResult>
{
    public string Id { get; set; } = default!;
}
