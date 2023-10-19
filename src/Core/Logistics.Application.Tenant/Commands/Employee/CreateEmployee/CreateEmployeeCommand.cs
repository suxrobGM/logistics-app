using Logistics.Shared.Enums;
using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class CreateEmployeeCommand : IRequest<ResponseResult>
{
    public string? UserId { get; set; }
    public string? Role { get; set; }
    public decimal Salary { get; set; }
    public SalaryType SalaryType { get; set; }
}
