namespace Logistics.Application.Tenant.Queries;

public sealed class GetEmployeeByIdQuery : RequestBase<ResponseResult<EmployeeDto>>
{
    public string? Id { get; set; }
}
