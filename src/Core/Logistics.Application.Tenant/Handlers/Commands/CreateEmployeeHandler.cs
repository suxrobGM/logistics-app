namespace Logistics.Application.Handlers.Commands;

internal sealed class CreateEmployeeHandler : RequestHandlerBase<CreateEmployeeCommand, DataResult>
{
    private readonly ITenantRepository<Employee> _employeeRepository;
    private readonly IMainRepository<User> _userRepository;
    private readonly IMainRepository<Tenant> _tenantRepository;

    public CreateEmployeeHandler(
        ITenantRepository<Employee> employeeRepository,
        IMainRepository<User> userRepository,
        IMainRepository<Tenant> tenantRepository)
    {
        _employeeRepository = employeeRepository;
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
    }

    protected override async Task<DataResult> HandleValidated(
        CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var existingEmployee = await _employeeRepository.GetAsync(i => i.Id == request.Id);
        var user = await _userRepository.GetAsync(i => i.Id == request.Id);
        var tenant = await _tenantRepository.GetAsync(i => i.Name == request.TenantId || i.Id == request.TenantId);
        
        if (tenant == null)
            return DataResult.CreateError($"Could not find the specified tenant '{request.TenantId}'");
        
        if (user == null)
            return DataResult.CreateError("Could not find the specified user");
        
        if (existingEmployee != null)
            return DataResult.CreateError("Employee already exists");
        
        user.JoinTenant(tenant.Id);

        var employee = new Employee()
        {
            Id = request.Id!,
            Role = EmployeeRole.Get(request.Role)!
        };
        await _employeeRepository.AddAsync(employee);
        _userRepository.Update(user);
        
        await _userRepository.UnitOfWork.CommitAsync();
        await _employeeRepository.UnitOfWork.CommitAsync();
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
        else if (EmployeeRole.Get(request.Role)! == null!)
        {
            errorDescription = "Invalid role name";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
