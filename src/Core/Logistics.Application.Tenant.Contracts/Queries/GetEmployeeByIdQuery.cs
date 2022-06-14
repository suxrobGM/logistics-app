namespace Logistics.Application.Contracts.Queries;

public sealed class GetEmployeeByIdQuery : RequestBase<DataResult<EmployeeDto>>
{
    public string? Id { get; set; }
}
