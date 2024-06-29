using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Tenant.Queries;

public class GetPayrollsQuery : SearchableQuery, IRequest<PagedResult<PayrollDto>>
{
    public string? EmployeeId { get; set; }
}
