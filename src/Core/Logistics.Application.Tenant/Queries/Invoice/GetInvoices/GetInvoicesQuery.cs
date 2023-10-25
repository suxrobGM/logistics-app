using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Tenant.Queries;

public class GetInvoicesQuery : PagedIntervalQuery, IRequest<PagedResponseResult<InvoiceDto>>
{
}
