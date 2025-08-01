using Logistics.Shared.Models;
using Logistics.Domain.Primitives.Enums;
using MediatR;

namespace Logistics.Application.Commands;

public class UpdateEmployeeCommand : IRequest<Result>
{
    public Guid UserId { get; set; }
    public string? Role { get; set; }
    public decimal? Salary { get; set; }
    public SalaryType? SalaryType { get; set; }
}
