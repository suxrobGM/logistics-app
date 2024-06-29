using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class CreatePayrollCommand : IRequest<Result>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string EmployeeId { get; set; } = default!;
}
