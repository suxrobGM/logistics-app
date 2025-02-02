using Logistics.Shared;
using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Queries;

public class GetPayrollsQuery : SearchableQuery, IRequest<PagedResult<PayrollDto>>
{
    public string? EmployeeId { get; set; }
}
