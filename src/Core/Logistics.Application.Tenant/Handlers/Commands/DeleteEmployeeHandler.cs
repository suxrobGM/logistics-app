namespace Logistics.Application.Handlers.Commands;

internal sealed class DeleteEmployeeHandler : RequestHandlerBase<DeleteEmployeeCommand, DataResult>
{
    private readonly ITenantRepository<Employee> _employeeRepository;

    public DeleteEmployeeHandler(ITenantRepository<Employee> employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    protected override async Task<DataResult> HandleValidated(DeleteEmployeeCommand request, CancellationToken cancellationToken)
    {
        _employeeRepository.Delete(request.Id!);
        await _employeeRepository.UnitOfWork.CommitAsync();
        return DataResult.CreateSuccess();
    }

    protected override bool Validate(DeleteEmployeeCommand request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.Id))
        {
            errorDescription = "Id is an empty string";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
