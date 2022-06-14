namespace Logistics.Application.Contracts.Queries;

public class GetEmployeeRoleQuery : RequestBase<DataResult<EmployeeRoleDto>>
{
    public string? UserId { get; set; }
}
