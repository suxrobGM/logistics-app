namespace Logistics.Application.Handlers.Queries;

internal sealed class GetEmployeesHandler : RequestHandlerBase<GetEmployeesQuery, PagedDataResult<EmployeeDto>>
{
    private readonly IMainRepository<User> _userRepository;
    private readonly ITenantRepository<Employee> _employeeRepository;

    public GetEmployeesHandler(
        IMainRepository<User> userRepository,
        ITenantRepository<Employee> employeeRepository)
    {
        _userRepository = userRepository;
        _employeeRepository = employeeRepository;
    }

    protected override Task<PagedDataResult<EmployeeDto>> HandleValidated(
        GetEmployeesQuery request, 
        CancellationToken cancellationToken)
    {
        var tenantId = _employeeRepository.CurrentTenant!.Id;
        var totalItems = _employeeRepository.GetQuery().Count();

        var filteredUsers = _userRepository
            .ApplySpecification(new SearchUsersByTenantId(request.Search, tenantId, request.OrderBy))
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToDictionary(user => user.Id);

        var userIds = filteredUsers.Keys.ToArray();
        
        var employeesEntity = _employeeRepository.GetQuery()
            .Where(i => userIds.Contains(i.Id))
            .ToArray();

        var employeesDtoArray = new EmployeeDto[employeesEntity.Length];

        for (var i = 0; i < employeesEntity.Length; i++)
        {
            var employee = employeesEntity[i];
            if (!filteredUsers.TryGetValue(employee.Id, out var user)) 
                continue;

            var employeeDto = new EmployeeDto(
                user.Id, 
                user.UserName, 
                user.FirstName, 
                user.LastName, 
                user.Email, 
                user.PhoneNumber, 
                employee.JoinedDate);
            
            var tenantRoles = employee.Roles.Select(role => new TenantRoleDto
            {
                Name = role.Name,
                DisplayName = role.DisplayName
            });
            employeeDto.Roles.AddRange(tenantRoles);
            employeesDtoArray[i] = employeeDto;
        }
        
        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);
        return Task.FromResult(new PagedDataResult<EmployeeDto>(employeesDtoArray, totalItems, totalPages));
    }

    protected override bool Validate(GetEmployeesQuery request, out string errorDescription)
    {
        errorDescription = string.Empty;
        var orderBy = request.OrderBy;

        if (string.IsNullOrEmpty(_employeeRepository.CurrentTenant?.Id))
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
        else if (!string.IsNullOrEmpty(orderBy) && !typeof(User).HasProperty(orderBy))
        {
            errorDescription = $"The specified '{orderBy}' is an invalid sort field name";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
