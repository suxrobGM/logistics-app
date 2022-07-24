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
        var totalItems = _employeeRepository.GetQuery().Count();
        var itemsQuery = _employeeRepository.GetQuery();

        if (!string.IsNullOrEmpty(request.Search))
        {
            itemsQuery = _employeeRepository.GetQuery(new SearchEmployeesSpecification(request.Search));
        }

        var employeesDto = itemsQuery
            .OrderBy(i => i.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(i => new EmployeeDto
            {
                Id = i.Id,
                ExternalId = i.ExternalId!,
                UserName = i.UserName!,
                FirstName = i.FirstName,
                LastName = i.LastName,
                Role = i.Role.Name,
                JoinedDate = i.JoinedDate
            })
            .ToArray();

        var employeeExtIds = employeesDto.Select(i => i.ExternalId);
        
        var users = _userRepository.GetQuery()
            .Where(user => employeeExtIds.Contains(user.Id))
            .ToDictionary(i => i.Id);

        foreach (var employee in employeesDto)
        {
            var user = users[employee.ExternalId!];
            employee.Email = user.Email;
            employee.PhoneNumber = user.PhoneNumber;
        }

        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);
        return Task.FromResult(new PagedDataResult<EmployeeDto>(employeesDto, totalItems, totalPages));
    }

    protected override bool Validate(GetEmployeesQuery request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (request.Page <= 0)
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
