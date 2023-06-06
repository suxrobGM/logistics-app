namespace Logistics.Application.Tenant.Queries;

internal sealed class GetEmployeesHandler : RequestHandlerBase<GetEmployeesRequest, PagedResponseResult<EmployeeDto>>
{
    private readonly IMainRepository _mainRepository;
    private readonly ITenantRepository _tenantRepository;

    public GetEmployeesHandler(
        IMainRepository mainRepository,
        ITenantRepository tenantRepository)
    {
        _mainRepository = mainRepository;
        _tenantRepository = tenantRepository;
    }

    protected override Task<PagedResponseResult<EmployeeDto>> HandleValidated(
        GetEmployeesRequest request, 
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantRepository.CurrentTenant!.Id;
        var totalItems = _tenantRepository.Query<Employee>().Count();
        var spec = new SearchUsersByTenantId(request.Search, tenantId, request.OrderBy, request.Descending);

        var filteredUsers = _mainRepository
            .ApplySpecification(spec)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToArray();

        var userIds = filteredUsers.Select(i => i.Id).ToArray();
        
        var employeesDict = _tenantRepository.Query<Employee>()
            .Where(i => userIds.Contains(i.Id))
            .ToDictionary(i => i.Id);

        var employeesDtoList = new List<EmployeeDto>();
        foreach (var user in filteredUsers)
        {
            if (!employeesDict.TryGetValue(user.Id, out var employee))
                continue;

            var employeeDto = new EmployeeDto(
                user.Id, 
                user.UserName!, 
                user.FirstName, 
                user.LastName, 
                user.Email!, 
                user.PhoneNumber, 
                employee.JoinedDate);
            
            var tenantRoles = employee.Roles.Select(role => new TenantRoleDto
            {
                Name = role.Name,
                DisplayName = role.DisplayName
            });
            employeeDto.Roles.AddRange(tenantRoles);
            employeesDtoList.Add(employeeDto);
        }

        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);
        return Task.FromResult(new PagedResponseResult<EmployeeDto>(employeesDtoList, totalItems, totalPages));
    }

    protected override bool Validate(GetEmployeesRequest request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(_tenantRepository.CurrentTenant?.Id))
        {
            errorDescription = "Could not evaluate current tenant's ID";
        }
        else if (request.Page <= 0)
        {
            errorDescription = "Page number should be non-negative";
        }
        else if (request.PageSize <= 1)
        {
            errorDescription = "Page size should be greater than one";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
