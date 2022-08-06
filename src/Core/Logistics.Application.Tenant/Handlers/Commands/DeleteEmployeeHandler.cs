namespace Logistics.Application.Handlers.Commands;

internal sealed class DeleteEmployeeHandler : RequestHandlerBase<DeleteEmployeeCommand, DataResult>
{
    private readonly IMainRepository<User> _userRepository;
    private readonly ITenantRepository<Employee> _employeeRepository;

    public DeleteEmployeeHandler(
        IMainRepository<User> userRepository,
        ITenantRepository<Employee> employeeRepository)
    {
        _userRepository = userRepository;
        _employeeRepository = employeeRepository;
    }

    protected override async Task<DataResult> HandleValidated(DeleteEmployeeCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _employeeRepository.CurrentTenant!.Id;
        var employee = await _employeeRepository.GetAsync(request.Id!);

        if (employee == null)
            return DataResult.CreateError($"Could not find employee with ID {request.Id}");

        var user = await _userRepository.GetAsync(i => i.Id == employee.Id);
        user?.RemoveTenant(tenantId);
        
        _employeeRepository.Delete(request.Id!);
        await _employeeRepository.UnitOfWork.CommitAsync();
        await _userRepository.UnitOfWork.CommitAsync();
        return DataResult.CreateSuccess();
    }

    protected override bool Validate(DeleteEmployeeCommand request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(_employeeRepository.CurrentTenant?.Id))
        {
            errorDescription = "Could not evaluate current tenant's ID";
        }
        else if (string.IsNullOrEmpty(request.Id))
        {
            errorDescription = "Id is an empty string";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
