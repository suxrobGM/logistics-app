using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Tenant.Queries;

public class GetPaymentsQuery : PagedIntervalQuery, IRequest<PagedResponseResult<PaymentDto>>
{
}
