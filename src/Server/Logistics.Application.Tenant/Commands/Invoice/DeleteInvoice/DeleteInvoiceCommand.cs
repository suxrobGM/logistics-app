using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class DeleteInvoiceCommand : IRequest<Result>
{
    public string Id { get; set; } = default!;
}
