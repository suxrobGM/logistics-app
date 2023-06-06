namespace Logistics.Application.Tenant.Queries;

internal sealed class GetEmployeeByIdHandler : RequestHandlerBase<GetEmployeeByIdQuery, ResponseResult<EmployeeDto>>
{
    private readonly IMainRepository _mainRepository;
    private readonly ITenantRepository _tenantRepository;

    public GetEmployeeByIdHandler(
        IMainRepository mainRepository,
        ITenantRepository tenantRepository)
    {
        _mainRepository = mainRepository;
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult<EmployeeDto>> HandleValidated(
        GetEmployeeByIdQuery request, CancellationToken cancellationToken)
    {
        var employeeEntity = await _tenantRepository.GetAsync<Employee>(request.Id);
        
        if (employeeEntity == null)
            return ResponseResult<EmployeeDto>.CreateError("Could not find the specified employee");

        var userEntity = await _mainRepository.GetAsync<User>(employeeEntity.Id);

        if (userEntity == null)
            return ResponseResult<EmployeeDto>.CreateError("Could not find the specified employee, the external ID is incorrect");

        var employee = new EmployeeDto
        {
            Id = employeeEntity.Id,
            UserName = userEntity.UserName!,
            FirstName = userEntity.FirstName,
            LastName = userEntity.LastName,
            Email = userEntity.Email,
            PhoneNumber = userEntity.PhoneNumber,
            JoinedDate = employeeEntity.JoinedDate
        };

        var tenantRoles = employeeEntity.Roles.Select(i => new TenantRoleDto
        {
            Name = i.Name,
            DisplayName = i.DisplayName
        });

        employee.Roles.AddRange(tenantRoles);
        return ResponseResult<EmployeeDto>.CreateSuccess(employee);
    }

    protected override bool Validate(GetEmployeeByIdQuery request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.Id))
        {
            errorDescription = "Id is an empty string";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
