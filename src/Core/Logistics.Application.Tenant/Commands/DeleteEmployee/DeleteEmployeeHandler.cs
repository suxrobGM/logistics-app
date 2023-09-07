namespace Logistics.Application.Tenant.Commands;

internal sealed class DeleteEmployeeHandler : RequestHandler<DeleteEmployeeCommand, ResponseResult>
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
        DeleteEmployeeCommand req, CancellationToken cancellationToken)
    {
        var tenant = _tenantRepository.GetCurrentTenant();
        var employee = await _tenantRepository.GetAsync<Employee>(req.UserId!);

        if (employee == null)
            return ResponseResult.CreateError($"Could not find employee with ID {req.UserId}");

        var user = await _mainRepository.GetAsync<User>(employee.Id);
        user?.RemoveTenant(tenant.Id);
        
        var employeeLoads = _tenantRepository.ApplySpecification(new GetEmployeeLoads(employee.Id));
        // var truck = await _tenantRepository.GetAsync<Truck>(i => i.DriverId == employee.Id);
        
        foreach (var load in employeeLoads)
        {
            if (load.AssignedDispatcherId == employee.Id)
            {
                load.AssignedDispatcher = null;
            }
        }
        
        _tenantRepository.Delete(employee);
        await _tenantRepository.UnitOfWork.CommitAsync();
        await _mainRepository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
    }
}
