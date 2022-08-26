using Logistics.Domain.Shared;

namespace Logistics.Application.Handlers.Queries;

public class GetDriversHandler : RequestHandlerBase<GetDriversQuery, PagedDataResult<EmployeeDto>>
{
    private readonly IMainRepository<User> _userRepository;
    private readonly ITenantRepository<Employee> _employeeRepository;
    private readonly ITenantRepository<TenantRole> _roleRepository;
    
    public GetDriversHandler(
        IMainRepository<User> userRepository,
        ITenantRepository<Employee> employeeRepository,
        ITenantRepository<TenantRole> roleRepository)
    {
        _userRepository = userRepository;
        _employeeRepository = employeeRepository;
        _roleRepository = roleRepository;
    }
    
    protected override async Task<PagedDataResult<EmployeeDto>> HandleValidated(GetDriversQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _employeeRepository.CurrentTenant!.Id;
        var totalItems = _employeeRepository.GetQuery().Count();
        var usersQuery = _userRepository.GetQuery();
        var driverRole = await _roleRepository.GetAsync(i => i.Name == TenantRoles.Driver);

        if (driverRole == null)
            return PagedDataResult<EmployeeDto>.CreateError("Could not found the driver role");

        if (!string.IsNullOrEmpty(request.Search))
        {
            usersQuery = _userRepository.GetQuery(new SearchUsersByTenantId(request.Search, tenantId));
        }

        var filteredUsers = usersQuery
            .OrderBy(i => i.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToDictionary(user => user.Id);

        var userIds = filteredUsers.Keys.ToArray();
        
        var employeesDto = _employeeRepository.GetQuery()
            .Where(i => userIds.Contains(i.Id) && i.Roles.Contains(driverRole))
            .Select(i => new EmployeeDto
            {
                Id = i.Id,
                JoinedDate = i.JoinedDate
            })
            .ToArray();

        foreach (var employee in employeesDto)
        {
            if (!filteredUsers.TryGetValue(employee.Id!, out var user))
                continue;
            
            employee.UserName = user.UserName;
            employee.FirstName = user.FirstName;
            employee.LastName = user.LastName;
            employee.Email = user.Email;
            employee.PhoneNumber = user.PhoneNumber;
        }

        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);
        return new PagedDataResult<EmployeeDto>(employeesDto, totalItems, totalPages);
    }

    protected override bool Validate(GetDriversQuery request, out string errorDescription)
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