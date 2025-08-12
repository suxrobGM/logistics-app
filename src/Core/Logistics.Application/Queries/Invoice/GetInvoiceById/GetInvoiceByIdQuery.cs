using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetInvoiceByIdQuery : IAppRequest<Result<InvoiceDto>>
{
    public Guid Id { get; set; }
}
