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
        var usersQuery = _userRepository.GetQuery();
        
        if (!string.IsNullOrEmpty(request.Search))
        {
            usersQuery = _userRepository.GetQuery(new SearchUsersByTenantIdSpecification(request.Search, tenantId));
        }

        var filteredUsers = usersQuery
            .OrderBy(i => i.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToDictionary(user => user.Id);

        var userIds = filteredUsers.Keys.ToArray();
        
        var employeesDto = _employeeRepository.GetQuery()
            .Where(i => userIds.Contains(i.ExternalId))
            .Select(i => new EmployeeDto
            {
                Id = i.Id,
                ExternalId = i.ExternalId!,
                Role = i.Role.Name,
                JoinedDate = i.JoinedDate
            })
            .ToArray();

        foreach (var employee in employeesDto)
        {
            var user = filteredUsers[employee.ExternalId];
            employee.UserName = user.UserName;
            employee.FirstName = user.FirstName;
            employee.LastName = user.LastName;
            employee.Email = user.Email;
            employee.PhoneNumber = user.PhoneNumber;
        }

        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);
        return Task.FromResult(new PagedDataResult<EmployeeDto>(employeesDto, totalItems, totalPages));
    }

    protected override bool Validate(GetEmployeesQuery request, out string errorDescription)
    {
        errorDescription = string.Empty;

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

        return string.IsNullOrEmpty(errorDescription);
    }
}
