using Logistics.Shared.Models;
using Logistics.Shared.Consts;
using MediatR;

namespace Logistics.Application.Commands;

public class CreateEmployeeCommand : IRequest<Result>
{
    public string UserId { get; set; } = null!;
    public string? Role { get; set; }
    public decimal Salary { get; set; }
    public SalaryType SalaryType { get; set; }
}
