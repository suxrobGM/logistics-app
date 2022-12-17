namespace Logistics.Application.Tenant.Handlers.Queries;

public class GetDriversHandler : RequestHandlerBase<GetDriversRequest, PagedResponseResult<EmployeeDto>>
{
    private readonly IMainRepository _mainRepository;
    private readonly ITenantRepository _tenantRepository;

    public GetDriversHandler(
        IMainRepository mainRepository,
        ITenantRepository tenantRepository)
    {
        _mainRepository = mainRepository;
        _tenantRepository = tenantRepository;
    }
    
    protected override async Task<PagedResponseResult<EmployeeDto>> HandleValidated(
        GetDriversRequest request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantRepository.CurrentTenant!.Id;
        var totalItems = _tenantRepository.Query<Employee>().Count();
        var driverRole = await _tenantRepository.GetAsync<TenantRole>(i => i.Name == TenantRoles.Driver);

        if (driverRole == null)
            return PagedResponseResult<EmployeeDto>.CreateError("Could not found the driver role");

        var filteredUsers = _mainRepository
            .ApplySpecification(new SearchUsersByTenantId(request.Search, tenantId, "Name"))
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToDictionary(user => user.Id);

        var userIds = filteredUsers.Keys.ToArray();
        
        var employeesDto = _tenantRepository.Query<Employee>()
            .Where(i => userIds.Contains(i.Id) && i.Roles.Contains(driverRole))
            .Select(i => new EmployeeDto
            {
                Id = i.Id,
                JoinedDate = i.JoinedDate
            })
            .ToArray();

        foreach (var employee in employeesDto)
        {
            if (!filteredUsers.TryGetValue(employee.Id, out var user))
                continue;
            
            employee.UserName = user.UserName!;
            employee.FirstName = user.FirstName;
            employee.LastName = user.LastName;
            employee.Email = user.Email;
            employee.PhoneNumber = user.PhoneNumber;
        }

        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);
        return new PagedResponseResult<EmployeeDto>(employeesDto, totalItems, totalPages);
    }

    protected override bool Validate(GetDriversRequest request, out string errorDescription)
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