using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Invoices.Queries;

public class GetInvoiceByIdQuery : IQuery<Result<InvoiceDto>>
{
    public Guid Id { get; set; }
}
