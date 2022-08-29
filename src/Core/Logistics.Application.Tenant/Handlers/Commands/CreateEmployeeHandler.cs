namespace Logistics.Application.Handlers.Commands;

internal sealed class CreateEmployeeHandler : RequestHandlerBase<CreateEmployeeCommand, DataResult>
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

    protected override async Task<DataResult> HandleValidated(
        CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var existingEmployee = await _tenantRepository.GetAsync<Employee>(i => i.Id == request.Id);
        var user = await _mainRepository.GetAsync<User>(i => i.Id == request.Id);
        var tenant = await _mainRepository.GetAsync<Tenant>(i => i.Name == request.TenantId || i.Id == request.TenantId);
        var tenantRole = await _tenantRepository.GetAsync<TenantRole>(i => i.Name == request.Role);
        
        if (tenant == null)
            return DataResult.CreateError($"Could not find the specified tenant '{request.TenantId}'");

        if (user == null)
            return DataResult.CreateError("Could not find the specified user");
        
        if (existingEmployee != null)
            return DataResult.CreateError("Employee already exists");
        
        user.JoinTenant(tenant.Id);

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
        return DataResult.CreateSuccess();
    }

    protected override bool Validate(CreateEmployeeCommand request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.Id))
        {
            errorDescription = "User Id is an empty string";
        }
        else if (string.IsNullOrEmpty(request.TenantId))
        {
            errorDescription = "TenantId is an empty string";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
