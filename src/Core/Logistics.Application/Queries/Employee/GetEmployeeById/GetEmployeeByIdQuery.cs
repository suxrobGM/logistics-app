using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Queries;

public class GetEmployeeByIdQuery : IRequest<Result<EmployeeDto>>
{
    public string UserId { get; set; } = null!;
}
