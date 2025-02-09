using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Queries;

public class CalculatePayrollQuery : IRequest<Result<PayrollDto>>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string EmployeeId { get; set; } = null!;
}
