using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetEmployeeByIdQuery : IQuery<Result<EmployeeDto>>
{
    public Guid UserId { get; set; }
}
