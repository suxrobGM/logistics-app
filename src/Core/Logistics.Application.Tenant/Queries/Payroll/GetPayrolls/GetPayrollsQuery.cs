using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Tenant.Queries;

public class GetPayrollsQuery : PagedQuery, IRequest<PagedResponseResult<PayrollDto>>
{
}
