using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Tenant.Queries;

public class GetPayrollsQuery : SearchableQuery, IRequest<PagedResponseResult<PayrollDto>>
{
    public string? EmployeeId { get; set; }
}
