using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Payments.Queries;

public class GetPaymentsQuery : PagedDateRangeQuery, IQuery<PagedResult<PaymentDto>>
{
}
