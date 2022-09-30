namespace Logistics.Application.Handlers.Commands;

internal sealed class UpdateEmployeeHandler : RequestHandlerBase<UpdateEmployeeCommand, DataResult>
{
    private readonly ITenantRepository _tenantRepository;

    public UpdateEmployeeHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<DataResult> HandleValidated(
        UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employeeEntity = await _tenantRepository.GetAsync<Employee>(request.Id);
        var tenantRole = await _tenantRepository.GetAsync<TenantRole>(i => i.Name == request.Role);

        if (employeeEntity == null)
            return DataResult.CreateError("Could not find the specified user");

        if (tenantRole != null)
        {
            employeeEntity.Roles.Clear();
            employeeEntity.Roles.Add(tenantRole);
        }
        
        _tenantRepository.Update(employeeEntity);
        await _tenantRepository.UnitOfWork.CommitAsync();
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
