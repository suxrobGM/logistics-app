namespace Logistics.Application.Tenant.Commands;

internal sealed class CreateEmployeeHandler : RequestHandler<CreateEmployeeCommand, ResponseResult>
{
    private readonly IMainRepository _mainRepository;
    private readonly ITenantRepository _tenantRepository;

    public CreateEmployeeHandler(
        IMainRepository mainRepository,
        ITenantRepository tenantRepository)
    {
        _mainRepository = mainRepository;
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult> HandleValidated(
        CreateEmployeeCommand req, CancellationToken cancellationToken)
    {
        var existingEmployee = await _tenantRepository.GetAsync<Employee>(req.UserId);
        
        if (existingEmployee != null)
            return ResponseResult.CreateError("Employee already exists");
        
        var user = await _mainRepository.GetAsync<User>(req.UserId);
        
        if (user == null)
            return ResponseResult.CreateError("Could not find the specified user");
        
        var tenantRole = await _tenantRepository.GetAsync<TenantRole>(i => i.Name == req.Role);
        var tenant = _tenantRepository.GetCurrentTenant();
        
        user.JoinTenant(tenant.Id);
        var employee = Employee.CreateEmployeeFromUser(user);

        if (tenantRole != null)
        {
            employee.Roles.Add(tenantRole);
        }
        
        await _tenantRepository.AddAsync(employee);
        _mainRepository.Update(user);
        
        await _mainRepository.UnitOfWork.CommitAsync();
        await _tenantRepository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
    }
}
