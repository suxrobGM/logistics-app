using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Identity.Employees.Queries;

public class GetEmployeeByIdQuery : IQuery<Result<EmployeeDto>>
{
    public Guid UserId { get; set; }
}
