namespace Logistics.Application.Tenant.Handlers.Commands;

internal sealed class CreateEmployeeHandler : RequestHandlerBase<CreateEmployeeCommand, ResponseResult>
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
        CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var existingEmployee = await _tenantRepository.GetAsync<Employee>(i => i.Id == request.Id);
        var user = await _mainRepository.GetAsync<User>(i => i.Id == request.Id);
        var tenantRole = await _tenantRepository.GetAsync<TenantRole>(i => i.Name == request.Role);
        var tenant = _tenantRepository.CurrentTenant;
        
        if (tenant == null)
            return ResponseResult.CreateError($"Could not find the tenant");

        if (user == null)
            return ResponseResult.CreateError("Could not find the specified user");
        
        if (existingEmployee != null)
            return ResponseResult.CreateError("Employee already exists");
        
        user.JoinedTenantIds.Add(tenant.Id);

        var employee = new Employee()
        {
            Id = request.Id!
        };

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

    protected override bool Validate(CreateEmployeeCommand request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.Id))
        {
            errorDescription = "User Id is an empty string";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
