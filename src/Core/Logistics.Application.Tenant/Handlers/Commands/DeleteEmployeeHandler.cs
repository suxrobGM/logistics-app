namespace Logistics.Application.Tenant.Handlers.Commands;

internal sealed class DeleteEmployeeHandler : RequestHandlerBase<DeleteEmployeeCommand, ResponseResult>
{
    private readonly IMainRepository _mainRepository;
    private readonly ITenantRepository _tenantRepository;

    public DeleteEmployeeHandler(
        IMainRepository mainRepository,
        ITenantRepository tenantRepository)
    {
        _mainRepository = mainRepository;
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult> HandleValidated(
        DeleteEmployeeCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantRepository.CurrentTenant!.Id;
        var employee = await _tenantRepository.GetAsync<Employee>(request.Id!);

        if (employee == null)
            return ResponseResult.CreateError($"Could not find employee with ID {request.Id}");

        var user = await _mainRepository.GetAsync<User>(employee.Id);
        user?.RemoveTenant(tenantId);
        
        _tenantRepository.Delete(employee);
        await _tenantRepository.UnitOfWork.CommitAsync();
        await _mainRepository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
    }

    protected override bool Validate(DeleteEmployeeCommand request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(_tenantRepository.CurrentTenant?.Id))
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
