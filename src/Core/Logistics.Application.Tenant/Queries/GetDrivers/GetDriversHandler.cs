using Logistics.Domain.Enums;
using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetDriversHandler : RequestHandler<GetDriversQuery, PagedResponseResult<EmployeeDto>>
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
        GetDriversQuery req, CancellationToken cancellationToken)
    {
        var tenant = _tenantRepository.GetCurrentTenant();
        var totalItems = _tenantRepository.Query<Employee>().Count();
        var driverRole = await _tenantRepository.GetAsync<TenantRole>(i => i.Name == TenantRoles.Driver);

        if (driverRole == null)
            return PagedResponseResult<EmployeeDto>.CreateError("Could not found the driver role");

        var filteredUsers = _mainRepository
            .ApplySpecification(new SearchUsersByTenantId(req.Search, tenant.Id, "Name"))
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
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
            
            employee.UserName = user.UserName;
            employee.FirstName = user.FirstName;
            employee.LastName = user.LastName;
            employee.FullName = $"{user.FirstName} {user.LastName}";
            employee.Email = user.Email;
            employee.PhoneNumber = user.PhoneNumber;
        }

        var totalPages = (int)Math.Ceiling(totalItems / (double)req.PageSize);
        return new PagedResponseResult<EmployeeDto>(employeesDto, totalItems, totalPages);
    }
}
