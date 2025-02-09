using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Queries;

public class GetInvoiceByIdQuery : IRequest<Result<InvoiceDto>>
{
    public string Id { get; set; } = null!;
}
