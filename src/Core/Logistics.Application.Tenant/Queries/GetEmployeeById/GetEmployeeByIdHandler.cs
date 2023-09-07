using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetEmployeeByIdHandler : RequestHandler<GetEmployeeByIdQuery, ResponseResult<EmployeeDto>>
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
        GetEmployeeByIdQuery req, CancellationToken cancellationToken)
    {
        var employeeEntity = await _tenantRepository.GetAsync<Employee>(req.Id);
        
        if (employeeEntity == null)
            return ResponseResult<EmployeeDto>.CreateError("Could not find the specified employee");

        var user = await _mainRepository.GetAsync<User>(employeeEntity.Id);

        if (user == null)
            return ResponseResult<EmployeeDto>.CreateError("Could not find the specified employee, the external ID is incorrect");

        var employee = new EmployeeDto
        {
            Id = employeeEntity.Id,
            UserName = user.UserName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = $"{user.FirstName} {user.LastName}",
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
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
