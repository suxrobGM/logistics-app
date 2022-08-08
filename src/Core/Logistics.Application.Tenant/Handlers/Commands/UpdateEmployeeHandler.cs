namespace Logistics.Application.Handlers.Commands;

internal sealed class UpdateEmployeeHandler : RequestHandlerBase<UpdateEmployeeCommand, DataResult>
{
    private readonly ITenantRepository<Employee> _employeeRepository;

    public UpdateEmployeeHandler(
        ITenantRepository<Employee> employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    protected override async Task<DataResult> HandleValidated(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employeeEntity = await _employeeRepository.GetAsync(request.Id!);

        if (employeeEntity == null)
            return DataResult.CreateError("Could not find the specified user");
        
        employeeEntity.Role = EmployeeRole.Get(request.Role)!;

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
        else if (EmployeeRole.Get(request.Role)! == null!)
        {
            errorDescription = "Invalid role name";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
