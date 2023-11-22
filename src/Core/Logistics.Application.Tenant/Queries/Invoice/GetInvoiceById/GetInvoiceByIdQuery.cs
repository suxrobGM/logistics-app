using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Tenant.Queries;

public class GetInvoiceByIdQuery : IRequest<ResponseResult<InvoiceDto>>
{
    public string Id { get; set; } = default!;
}
