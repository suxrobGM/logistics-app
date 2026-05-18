using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Payments.Queries;

public class GetPaymentsQuery : PagedIntervalQuery, IQuery<PagedResult<PaymentDto>>
{
}
