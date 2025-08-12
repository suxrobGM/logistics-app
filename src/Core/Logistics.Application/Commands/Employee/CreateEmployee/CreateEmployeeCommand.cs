using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

using MediatR;

namespace Logistics.Application.Commands;

public class CreateEmployeeCommand : IRequest<Result>
{
    public Guid UserId { get; set; }
    public string? Role { get; set; }
    public decimal Salary { get; set; }
    public SalaryType SalaryType { get; set; }
}
