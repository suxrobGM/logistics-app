namespace Logistics.Application.Handlers.Queries;

internal sealed class GetEmployeeRoleHandler : RequestHandlerBase<GetEmployeeRoleQuery, DataResult<EmployeeRoleDto>>
{
    private readonly ITenantRepository<Employee> _employeeRepository;

    public GetEmployeeRoleHandler(ITenantRepository<Employee> employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    protected override async Task<DataResult<EmployeeRoleDto>> HandleValidated(GetEmployeeRoleQuery request, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetAsync(i => i.Id == request.UserId || i.ExternalId == request.UserId);

        if (employee == null)
            return DataResult<EmployeeRoleDto>.CreateError("Could not find the user");
        

        var employeeRole = new EmployeeRoleDto(employee.Id, employee.Role.Name);
        return DataResult<EmployeeRoleDto>.CreateSuccess(employeeRole);
    }

    protected override bool Validate(GetEmployeeRoleQuery request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.UserId))
        {
            errorDescription = "User ID is an empty string";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
