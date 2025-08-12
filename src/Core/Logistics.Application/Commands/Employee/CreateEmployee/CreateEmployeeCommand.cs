using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Commands;

public class CreateEmployeeCommand : IAppRequest
{
    public Guid UserId { get; set; }
    public string? Role { get; set; }
    public decimal Salary { get; set; }
    public SalaryType SalaryType { get; set; }
}
