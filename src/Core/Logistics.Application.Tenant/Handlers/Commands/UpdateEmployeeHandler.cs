namespace Logistics.Application.Handlers.Commands;

internal sealed class UpdateEmployeeHandler : RequestHandlerBase<UpdateEmployeeCommand, DataResult>
{
    private readonly ITenantRepository<Employee> _employeeRepository;
    private readonly ITenantRepository<TenantRole> _roleRepository;

    public UpdateEmployeeHandler(
        ITenantRepository<Employee> employeeRepository,
        ITenantRepository<TenantRole> roleRepository)
    {
        _employeeRepository = employeeRepository;
        _roleRepository = roleRepository;
    }

    protected override async Task<DataResult> HandleValidated(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employeeEntity = await _employeeRepository.GetAsync(request.Id!);
        var tenantRole = await _roleRepository.GetAsync(i => i.Name == request.Role);
        
        if (tenantRole == null)
            return DataResult.CreateError("Invalid role name");

        if (employeeEntity == null)
            return DataResult.CreateError("Could not find the specified user");
        
        employeeEntity.Roles.Add(tenantRole);
        _employeeRepository.Update(employeeEntity);
        await _employeeRepository.UnitOfWork.CommitAsync();
        return DataResult.CreateSuccess();
    }

    protected override bool Validate(UpdateEmployeeCommand request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.Id))
        {
            errorDescription = "Id is an empty string";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
