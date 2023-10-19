using Logistics.Shared.Enums;

namespace Logistics.Application.Tenant.Commands;

public class CreateEmployeeCommand : Request<ResponseResult>
{
    public string? UserId { get; set; }
    public string? Role { get; set; }
    public decimal Salary { get; set; }
    public SalaryType SalaryType { get; set; }
}
