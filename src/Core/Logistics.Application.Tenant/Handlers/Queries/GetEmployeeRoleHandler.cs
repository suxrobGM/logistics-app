namespace Logistics.Application.Handlers.Queries;

internal sealed class GetEmployeeRoleHandler : RequestHandlerBase<GetEmployeeRoleQuery, DataResult<EmployeeRoleDto>>
{
    private readonly ITenantRepository<Employee> _repository;

    public GetEmployeeRoleHandler(ITenantRepository<Employee> repository)
    {
        _repository = repository;
    }

    protected override async Task<DataResult<EmployeeRoleDto>> HandleValidated(GetEmployeeRoleQuery request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetAsync(i => i.Id == request.UserId || i.ExternalId == request.UserId);

        if (user == null)
        {
            return DataResult<EmployeeRoleDto>.CreateError("Could not find the user");
        }

        var userRole = new EmployeeRoleDto(user.Id, user.Role.Name);
        return DataResult<EmployeeRoleDto>.CreateSuccess(userRole);
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
