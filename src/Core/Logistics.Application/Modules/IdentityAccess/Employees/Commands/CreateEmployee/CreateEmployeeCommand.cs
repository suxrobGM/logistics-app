using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.IdentityAccess.Employees.Commands;

public class CreateEmployeeCommand : ICommand
{
    public Guid UserId { get; set; }
    public string? Role { get; set; }
    public decimal Salary { get; set; }
    public SalaryType SalaryType { get; set; }
}
