using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetPaymentsQuery : PagedIntervalQuery, IAppRequest<PagedResult<PaymentDto>>
{
}
