using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Application.Modules.IdentityAccess.Employees.Commands;

public class UpdateEmployeeCommand : ICommand
{
    public Guid UserId { get; set; }
    public string? Role { get; set; }
    public decimal? Salary { get; set; }
    public SalaryType? SalaryType { get; set; }
    public EmployeeStatus? Status { get; set; }
    public Address? Address { get; set; }
}
