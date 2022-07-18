namespace Logistics.Application.Handlers.Commands;

internal sealed class CreateEmployeeHandler : RequestHandlerBase<CreateEmployeeCommand, DataResult>
{
    private readonly IMapper _mapper;
    private readonly ITenantRepository<Employee> _employeeRepository;
    private readonly IMainRepository<User> _userRepository;
    private readonly IMainRepository<Tenant> _tenantRepository;

    public CreateEmployeeHandler(
        IMapper mapper,
        ITenantRepository<Employee> employeeRepository,
        IMainRepository<User> userRepository,
        IMainRepository<Tenant> tenantRepository)
    {
        _mapper = mapper;
        _employeeRepository = employeeRepository;
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
    }

    protected override async Task<DataResult> HandleValidated(
        CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = _mapper.Map<Employee>(request);
        var existingEmployee = await _employeeRepository.GetAsync(i => i.ExternalId == request.ExternalId || i.UserName == request.UserName);
        var user = await _userRepository.GetAsync(i => i.Id == employee.ExternalId);
        var tenant = await _tenantRepository.GetAsync(i => i.Name == request.TenantId || i.Id == request.TenantId);
        
        if (tenant == null)
            return DataResult.CreateError($"Could not find the specified tenant '{request.TenantId}'");
        
        if (user == null)
            return DataResult.CreateError("Could not find the specified user");
        
        if (existingEmployee != null)
            return DataResult.CreateError("Employee already exists");
        
        user.JoinedTenants.Add(tenant.Id);
        await _employeeRepository.AddAsync(employee);
        _userRepository.Update(user);
        await _userRepository.UnitOfWork.CommitAsync();
        await _employeeRepository.UnitOfWork.CommitAsync();
        return DataResult.CreateSuccess();
    }

    protected override bool Validate(CreateEmployeeCommand request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.ExternalId))
        {
            errorDescription = "External Id is an empty string";
        }
        else if (string.IsNullOrEmpty(request.UserName))
        {
            errorDescription = "UserName is an empty string";
        }
        else if (string.IsNullOrEmpty(request.TenantId))
        {
            errorDescription = "TenantId is an empty string";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
